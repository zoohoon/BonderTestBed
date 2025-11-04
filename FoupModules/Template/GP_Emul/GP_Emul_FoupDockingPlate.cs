using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System;

namespace FoupModules.DockingPlate
{
    public class GP_Emul_FOUPDockingPlate : FoupDockingPlateBase, ITemplateModule, IFactoryModule
    {
        private GP_Emul_FOUPDockingPlateState _State;

        public GP_Emul_FOUPDockingPlateState State
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
        public GP_Emul_FOUPDockingPlate(IFoupModule module) : base(module)
        {
        }
        public GP_Emul_FOUPDockingPlate() : base()
        {
        }
        public void StateTransition(GP_Emul_FOUPDockingPlateState state)
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
                int ret = FoupIOManager.MonitorForIO(
                FoupIOManager.IOMap.Inputs.DI_CST_LOCK12s[Module.FoupIndex], true, 100, 200);
                if (ret != 1)
                {
                    StateTransition(new GP_Emul_DockingPlateUnLock(this));
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    StateTransition(new GP_Emul_DockingPlateLock(this));
                    retVal = EventCodeEnum.NONE;
                }
                LoggerManager.Debug($"GP_Emul_DockingPlate.CheckState(): Initial state is {_State.GetState()}");

            }
            catch (Exception err)
            {
                StateTransition(new GP_Emul_DockingPlateError(this));
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. GP_Emul_DockingPlate - CheckState() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
                return retVal;
            }

            return retVal;
        }
        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                State = new GP_Emul_DockingPlateLock(this);

                Inputs.Add(FoupIOManager.IOMap.Inputs.DI_CST12_PRESs[Module.FoupIndex]);
                Inputs.Add(FoupIOManager.IOMap.Inputs.DI_CST12_PRES2s[Module.FoupIndex]);
                Inputs.Add(FoupIOManager.IOMap.Inputs.DI_CST_LOCK12s[Module.FoupIndex]);
                Inputs.Add(FoupIOManager.IOMap.Inputs.DI_CST_UNLOCK12s[Module.FoupIndex]);

                Outputs.Add(FoupIOManager.IOMap.Outputs.DO_CST_12INCH_LOCKs[Module.FoupIndex]);
                Outputs.Add(FoupIOManager.IOMap.Outputs.DO_CST_12INCH_UNLOCKs[Module.FoupIndex]);

                //FoupIOManager.IOMap.Inputs.DI_CST12_PRESs[Module.FoupIndex].PortIndex.Value = -1;
                //FoupIOManager.IOMap.Inputs.DI_CST12_PRES2s[Module.FoupIndex].PortIndex.Value = -1;
                //FoupIOManager.IOMap.Inputs.DI_CST_LOCK12s[Module.FoupIndex].PortIndex.Value = -1;
                //FoupIOManager.IOMap.Inputs.DI_CST_UNLOCK12s[Module.FoupIndex].PortIndex.Value = -1;
                //FoupIOManager.IOMap.Outputs.DO_CST_12INCH_LOCKs[Module.FoupIndex].PortIndex.Value = -1;
                //FoupIOManager.IOMap.Outputs.DO_CST_12INCH_UNLOCKs[Module.FoupIndex].PortIndex.Value = -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
    public abstract class GP_Emul_FOUPDockingPlateState
    {

        public GP_Emul_FOUPDockingPlateState(GP_Emul_FOUPDockingPlate _module)
        {
            PlateModule = _module;
        }

        private GP_Emul_FOUPDockingPlate _PlateModule;

        public GP_Emul_FOUPDockingPlate PlateModule
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
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (retVal == EventCodeEnum.NONE)
                {
                    PlateModule.StateTransition(new GP_Emul_DockingPlateLock(PlateModule));
                }
                else
                {
                    PlateModule.StateTransition(new GP_Emul_DockingPlateError(PlateModule));
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;

        }
        protected EventCodeEnum UnLockMethod()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (retVal == EventCodeEnum.NONE)
                {
                    PlateModule.StateTransition(new GP_Emul_DockingPlateUnLock(PlateModule));
                }
                else
                {
                    PlateModule.StateTransition(new GP_Emul_DockingPlateError(PlateModule));
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class GP_Emul_DockingPlateLock : GP_Emul_FOUPDockingPlateState
    {
        public GP_Emul_DockingPlateLock(GP_Emul_FOUPDockingPlate module) : base(module)
        {
            module.EnumState = DockingPlateStateEnum.LOCK;

            int foupindex = module.Module.FoupIndex;

            FoupInputPortDefinitions inputs = module.Module.IOManager.IOMap.Inputs;
            FoupOutputPortDefinitions outputs = module.Module.IOManager.IOMap.Outputs;

            //inputs.DI_CST12_PRESs[foupindex].ForcedIO.IsForced = true;
            //inputs.DI_CST12_PRESs[foupindex].ForcedIO.ForecedValue = true;
            //inputs.DI_CST12_PRES2s[foupindex].ForcedIO.IsForced = true;
            //inputs.DI_CST12_PRES2s[foupindex].ForcedIO.ForecedValue = true;

            inputs.DI_CST_LOCK12s[foupindex].ForcedIO.IsForced = true;
            inputs.DI_CST_LOCK12s[foupindex].ForcedIO.ForecedValue = true;
            inputs.DI_CST_UNLOCK12s[foupindex].ForcedIO.IsForced = true;
            inputs.DI_CST_UNLOCK12s[foupindex].ForcedIO.ForecedValue = false;

            outputs.DO_CST_12INCH_LOCKs[foupindex].Value = true;
            outputs.DO_CST_12INCH_UNLOCKs[foupindex].Value = false;

            outputs.DO_CST_IND_AUTOs[foupindex].Value = true;
            outputs.DO_CST_IND_UNLOADs[foupindex].Value = true;
            outputs.DO_CST_IND_LOADs[foupindex].Value = false;

            //outputs.DO_CST_IND_PLACEMENTs[foupindex].Value = true;
            //outputs.DO_CST_IND_PRESENCEs[foupindex].Value = true;
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
    public class GP_Emul_DockingPlateUnLock : GP_Emul_FOUPDockingPlateState
    {
        public GP_Emul_DockingPlateUnLock(GP_Emul_FOUPDockingPlate module) : base(module)
        {
            module.EnumState = DockingPlateStateEnum.UNLOCK;

            int foupindex = module.Module.FoupIndex;

            FoupInputPortDefinitions inputs = module.Module.IOManager.IOMap.Inputs;
            FoupOutputPortDefinitions outputs = module.Module.IOManager.IOMap.Outputs;

            //inputs.DI_CST12_PRESs[foupindex].ForcedIO.IsForced = true;
            //inputs.DI_CST12_PRESs[foupindex].ForcedIO.ForecedValue = false;
            //inputs.DI_CST12_PRES2s[foupindex].ForcedIO.IsForced = true;
            //inputs.DI_CST12_PRES2s[foupindex].ForcedIO.ForecedValue = false;
            inputs.DI_CST_LOCK12s[foupindex].ForcedIO.IsForced = true;
            inputs.DI_CST_LOCK12s[foupindex].ForcedIO.ForecedValue = false;
            inputs.DI_CST_UNLOCK12s[foupindex].ForcedIO.IsForced = true;
            inputs.DI_CST_UNLOCK12s[foupindex].ForcedIO.ForecedValue = true;
            
            outputs.DO_CST_12INCH_LOCKs[foupindex].Value = false;
            outputs.DO_CST_12INCH_UNLOCKs[foupindex].Value = true;

            outputs.DO_CST_IND_AUTOs[foupindex].Value = true;
            outputs.DO_CST_IND_UNLOADs[foupindex].Value = false;
            outputs.DO_CST_IND_LOADs[foupindex].Value = false;

            //outputs.DO_CST_IND_PLACEMENTs[foupindex].Value = false;
            //outputs.DO_CST_IND_PRESENCEs[foupindex].Value = false;
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
    public class GP_Emul_DockingPlateError : GP_Emul_FOUPDockingPlateState
    {
        public GP_Emul_DockingPlateError(GP_Emul_FOUPDockingPlate module) : base(module)
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
