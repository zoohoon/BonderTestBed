using CylType;
using FoupModules.DockingPlate;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System;

namespace FoupModules.Template.TOP
{
    public class TOPFoupDockingPlate : FoupDockingPlateBase, ITemplateModule
    {
        #region // ITemplateModule implementation.
        public bool Initialized { get; set; } = false;
        public void DeInitModule()
        {

        }

        public TOPFoupDockingPlate()
        {

        }

        public EventCodeEnum InitModule()
        {
            //EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            //ret = StateInit();
            //if (ret != EventCodeEnum.NONE)
            //{
            //    LoggerManager.Debug($"TOPFoupDockingPlate.InitModule(): Init. error. Ret = {ret}");
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

        private TOPFoupDockingPlateStateBase _State;

        public TOPFoupDockingPlateStateBase State
        {
            get { return _State; }
            set { _State = value; }
        }

        public TOPFoupDockingPlate(FoupModule module) : base(module)
        {
            StateInit();
        }

        public void StateTransition(TOPFoupDockingPlateStateBase state)
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            return retVal;
        }
        public override EventCodeEnum Lock()
        {
            return State.Lock();
        }

        public override EventCodeEnum Unlock()
        {
            return State.Unlock();
        }
        public override EventCodeEnum RecoveryUnlock()
        {
            return State.RecoveryUnlock();
        }
        public override DockingPlateStateEnum GetState()
        {
            return State.GetState();
        }

        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            var IO = Module.IOManager;
            int retValue = -1;

            try
            {
                if (Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6)
                {
                    // TODO

                    bool PLACEMENT_6Inch_value;
                    bool PRESENCE_6_8Inch_value;
                    bool NPLACEMENT_6_8Inch_value;

                    int ret = IO.ReadBit(IO.IOMap.Inputs.DI_C6IN_PLACEMENT, out PLACEMENT_6Inch_value);
                    int ret1 = IO.ReadBit(IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE1, out PRESENCE_6_8Inch_value);
                    int ret2 = IO.ReadBit(IO.IOMap.Inputs.DI_C6IN_C8IN_NPLACEMENT, out NPLACEMENT_6_8Inch_value);

                    if (ret != 0)
                    {
                        State = new TOPFoupDockingPlateStateError(this);
                    }
                    else
                    {
                        if (PLACEMENT_6Inch_value == true)
                        {
                            if (PRESENCE_6_8Inch_value != true)
                            {
                                retValue = FoupCylinderType.FoupDockingPlate6.Retract();

                                if (NPLACEMENT_6_8Inch_value == true)
                                {
                                    State = new TOPFoupDockingPlateStateUnLock(this);
                                }
                                else
                                {
                                    State = new TOPFoupDockingPlateStateError(this);
                                }
                            }
                            else
                            {
                                // TODO
                                retValue = FoupCylinderType.FoupDockingPlate6.Extend();
                                State = new TOPFoupDockingPlateStateLock(this);
                            }
                        }
                        else
                        {
                            retValue = FoupCylinderType.FoupDockingPlate6.Retract();

                            if (NPLACEMENT_6_8Inch_value == true)
                            {
                                State = new TOPFoupDockingPlateStateUnLock(this);
                            }
                            else
                            {
                                State = new TOPFoupDockingPlateStateError(this);
                            }
                        }
                    }
                }
                else if (Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                {
                    bool PLACEMENT_8Inch_value;
                    bool PRESENCE_6_8Inch_value;
                    bool NPLACEMENT_6_8Inch_value;

                    int ret = IO.ReadBit(IO.IOMap.Inputs.DI_C8IN_PLACEMENT, out PLACEMENT_8Inch_value);
                    int ret1 = IO.ReadBit(IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE1, out PRESENCE_6_8Inch_value);
                    int ret2 = IO.ReadBit(IO.IOMap.Inputs.DI_C6IN_C8IN_NPLACEMENT, out NPLACEMENT_6_8Inch_value);

                    if (ret != 0)
                    {
                        State = new TOPFoupDockingPlateStateError(this);
                    }
                    else
                    {
                        // LOCK 됐을 때
                        if (PLACEMENT_8Inch_value == true)
                        {
                            // 카세트가 놓여있지 않으면
                            if (PRESENCE_6_8Inch_value != true)
                            {
                                retValue = FoupCylinderType.FoupDockingPlate8.Retract();

                                if (NPLACEMENT_6_8Inch_value == true)
                                {
                                    State = new TOPFoupDockingPlateStateUnLock(this);
                                }
                                else
                                {
                                    State = new TOPFoupDockingPlateStateError(this);
                                }
                            }
                            // 카세트가 놓여있으면
                            else
                            {
                                // TODO
                                retValue = FoupCylinderType.FoupDockingPlate8.Extend();
                                State = new TOPFoupDockingPlateStateLock(this);
                            }
                        }
                        else
                        {
                            //retValue = FoupCylinderType.FoupDockingPlate8.Retract();

                            // Unlock 됐을 때
                            if (NPLACEMENT_6_8Inch_value == true)
                            {
                                State = new TOPFoupDockingPlateStateUnLock(this);
                            }
                            else
                            {
                                State = new TOPFoupDockingPlateStateError(this);
                            }
                        }
                    }
                }
                else if (Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                {
                    int ret = IO.MonitorForIO(IO.IOMap.Inputs.DI_C12IN_NPLACEMENT, true, 100, 1000);

                    if (ret == 0)
                    {
                        StateTransition(new TOPFoupDockingPlateStateUnLock(this));
                    }
                    else if (ret == -2)
                    {
                        int ret2 = IO.MonitorForIO(IO.IOMap.Inputs.DI_C12IN_NPLACEMENT, false, 100, 1000);

                        if (ret2 == 0)
                        {
                            StateTransition(new TOPFoupDockingPlateStateLock(this));
                        }
                        else
                        {
                            StateTransition(new TOPFoupDockingPlateStateError(this));
                        }
                    }
                    else
                    {
                        StateTransition(new TOPFoupDockingPlateStateError(this));
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{Module.DeviceParam.SubstrateSize.Value}]");
                    StateTransition(new TOPFoupDockingPlateStateError(this));
                }

                if (EnumState == DockingPlateStateEnum.ERROR)
                {
                    retval = EventCodeEnum.FOUP_ERROR;
                }
                else
                {
                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                StateTransition(new TOPFoupDockingPlateStateError(this));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
    }
    public abstract class TOPFoupDockingPlateStateBase
    {
        public TOPFoupDockingPlateStateBase(TOPFoupDockingPlate owner)
        {
            this.Owner = owner;
        }

        private TOPFoupDockingPlate _Owner;
        public TOPFoupDockingPlate Owner
        {
            get { return _Owner; }
            set
            {
                if (value != _Owner)
                {
                    _Owner = value;
                }
            }
        }

        protected IFoupModule FoupModule => Owner.Module;

        protected IFoupIOStates IO => Owner.Module.IOManager;

        public abstract DockingPlateStateEnum GetState();

        public abstract EventCodeEnum Lock();

        public abstract EventCodeEnum Unlock();
        public virtual EventCodeEnum RecoveryUnlock() { return EventCodeEnum.NONE; }
        public EventCodeEnum LockFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            int retIOvalue = -1;
            int retIOvalue1 = -1;
            int retIOvalue2 = -1;
            var IO = Owner.Module.IOManager;
            FoupCylinderType DockingPlateCylinder = null;
            IOPortDescripter<bool> Presence_IO = null;
            IOPortDescripter<bool> PosA_IO = null;
            IOPortDescripter<bool> PosB_IO = null;
            IOPortDescripter<bool> CP_OUT_IO = null;
            bool Level = true;
            long maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
            long timeout = FoupIOGlobalVar.IO_CHECK_TIME_OUT;

            try
            {
                switch (FoupModule.DeviceParam.SubstrateSize.Value)
                {
                    case SubstrateSizeEnum.UNDEFINED:
                        break;
                    case SubstrateSizeEnum.INCH6:
                        DockingPlateCylinder = FoupCylinderType.FoupDockingPlate6;
                        Presence_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE1;
                        CP_OUT_IO = IO.IOMap.Inputs.DI_CP_OUT;

                        Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = FoupIOGlobalVar.IO_CHECK_TIME_OUT;
                        break;
                    case SubstrateSizeEnum.INCH8:
                        DockingPlateCylinder = FoupCylinderType.FoupDockingPlate8;
                        Presence_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE1;
                        CP_OUT_IO = IO.IOMap.Inputs.DI_CP_OUT;

                        Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = 3000;
                        break;
                    case SubstrateSizeEnum.INCH12:
                        DockingPlateCylinder = FoupCylinderType.FoupDockingPlate12;
                        Presence_IO = IO.IOMap.Inputs.DI_C12IN_PRESENCE1;
                        PosA_IO = IO.IOMap.Inputs.DI_C12IN_POSA;
                        PosB_IO = IO.IOMap.Inputs.DI_C12IN_POSB;
                        CP_OUT_IO = IO.IOMap.Inputs.DI_CP_OUT;

                        Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = 3000;
                        break;
                    case SubstrateSizeEnum.CUSTOM:
                        break;
                    default:
                        break;
                }

                if ((DockingPlateCylinder != null) && (Presence_IO != null) && (CP_OUT_IO != null))
                {
                    //6, 8inch locking
                    if (Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6 ||
                       Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                    {
                        retIOvalue = IO.MonitorForIO(Presence_IO, Level, maintaintime, timeout);
                        //there is cassette
                        if (retIOvalue == 0)
                        {
                            retIOvalue = IO.MonitorForIO(CP_OUT_IO, Level, maintaintime, timeout);

                            if (retIOvalue == 0)
                            {
                                retIOvalue = DockingPlateCylinder.Extend();

                                if (retIOvalue == 0)
                                {
                                    Owner.StateTransition(new TOPFoupDockingPlateStateLock(Owner));
                                }
                                else
                                {
                                    // TODO : Why Retract function is called?
                                    retIOvalue = DockingPlateCylinder.Retract();
                                    Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                                }
                            }
                        }
                        //there is not cassette
                        else if (retIOvalue == -2)
                        {
                            retIOvalue = IO.MonitorForIO(Presence_IO, false, maintaintime, timeout);

                            if (retIOvalue == 0)
                            {
                                retIOvalue = IO.MonitorForIO(CP_OUT_IO, Level, maintaintime, timeout);

                                if (retIOvalue == 0)
                                {
                                    retIOvalue = DockingPlateCylinder.Retract();

                                    if (retIOvalue == 0)
                                    {
                                        Owner.StateTransition(new TOPFoupDockingPlateStateLock(Owner));
                                    }
                                    else
                                    {
                                        Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                                    }
                                }
                            }
                            else
                            {
                                Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                            }
                        }
                        else
                        {
                            Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                        }
                    }
                    //12 inch locking
                    else
                    {
                        retIOvalue = IO.MonitorForIO(Presence_IO, Level, maintaintime, timeout);
                        retIOvalue1 = IO.MonitorForIO(PosA_IO, Level, maintaintime, timeout);
                        retIOvalue2 = IO.MonitorForIO(PosB_IO, Level, maintaintime, timeout);
                        //there is cassette
                        if (retIOvalue == 0 && retIOvalue1 == 0 && retIOvalue2 == 0)
                        {
                            retIOvalue = IO.MonitorForIO(CP_OUT_IO, Level, maintaintime, timeout);

                            if (retIOvalue == 0)
                            {
                                retIOvalue = DockingPlateCylinder.Extend();

                                if (retIOvalue == 0)
                                {
                                    Owner.StateTransition(new TOPFoupDockingPlateStateLock(Owner));
                                }
                                else
                                {
                                    // TODO : Why Retract function is called?
                                    retIOvalue = DockingPlateCylinder.Retract();
                                    Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                                }
                            }
                        }
                        //there is not cassette
                        else if (retIOvalue == -2 && retIOvalue1 == -2 && retIOvalue2 == -2)
                        {
                            retIOvalue = IO.MonitorForIO(Presence_IO, false, maintaintime, timeout);
                            retIOvalue1 = IO.MonitorForIO(PosA_IO, false, maintaintime, timeout);
                            retIOvalue2 = IO.MonitorForIO(PosB_IO, false, maintaintime, timeout);

                            if (retIOvalue == 0 && retIOvalue1 == 0 && retIOvalue2 == 0)
                            {
                                retIOvalue = IO.MonitorForIO(CP_OUT_IO, Level, maintaintime, timeout);

                                if (retIOvalue == 0)
                                {
                                    retIOvalue = DockingPlateCylinder.Retract();

                                    if (retIOvalue == 0)
                                    {
                                        Owner.StateTransition(new TOPFoupDockingPlateStateLock(Owner));
                                    }
                                    else
                                    {
                                        Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                                    }
                                }
                            }
                            else
                            {
                                Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                            }
                        }
                        else
                        {
                            Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                        }
                    }
                    //retIOvalue = IO.MonitorForIO(Presence_IO, Level, maintaintime, timeout);

                    //if (retIOvalue == 0)
                    //{
                    //    retIOvalue = DockingPlateCylinder.Extend();

                    //    if (retIOvalue == 0)
                    //    {
                    //        Owner.StateTransition(new TOPFoupDockingPlateStateLock(Owner));
                    //    }
                    //    else
                    //    {
                    //        // TODO : Why Retract function is called?
                    //        retIOvalue = DockingPlateCylinder.Retract();
                    //        Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                    //    }
                    //}
                    //else
                    //{
                    //    Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                    //}
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{FoupModule.DeviceParam.SubstrateSize.Value}]");
                    Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                }

                if (Owner.EnumState != DockingPlateStateEnum.ERROR)
                {
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    retval = EventCodeEnum.FOUP_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }

        public EventCodeEnum UnlockFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            int retIOvalue = -1;
            int retIOvalue1 = -1;
            int retIOvalue2 = -1;
            var IO = Owner.Module.IOManager;
            FoupCylinderType DockingPlateCylinder = null;
            IOPortDescripter<bool> Presence_IO = null;
            IOPortDescripter<bool> PosA_IO = null;
            IOPortDescripter<bool> PosB_IO = null;
            IOPortDescripter<bool> CP_OUT_IO = null;
            bool Level = true;
            long maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
            long timeout = FoupIOGlobalVar.IO_CHECK_TIME_OUT;

            try
            {
                switch (FoupModule.DeviceParam.SubstrateSize.Value)
                {
                    case SubstrateSizeEnum.UNDEFINED:
                        break;
                    case SubstrateSizeEnum.INCH6:
                        DockingPlateCylinder = FoupCylinderType.FoupDockingPlate6;
                        Presence_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE1;
                        CP_OUT_IO = IO.IOMap.Inputs.DI_CP_OUT;

                        Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = FoupIOGlobalVar.IO_CHECK_TIME_OUT;
                        break;
                    case SubstrateSizeEnum.INCH8:
                        DockingPlateCylinder = FoupCylinderType.FoupDockingPlate8;
                        Presence_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE1;
                        CP_OUT_IO = IO.IOMap.Inputs.DI_CP_OUT;

                        Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = 3000;
                        break;
                    case SubstrateSizeEnum.INCH12:
                        DockingPlateCylinder = FoupCylinderType.FoupDockingPlate12;
                        Presence_IO = IO.IOMap.Inputs.DI_C12IN_PRESENCE1;
                        PosA_IO = IO.IOMap.Inputs.DI_C12IN_POSA;
                        PosB_IO = IO.IOMap.Inputs.DI_C12IN_POSB;
                        CP_OUT_IO = IO.IOMap.Inputs.DI_CP_OUT;

                        Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = 3000;
                        break;
                    case SubstrateSizeEnum.CUSTOM:
                        break;
                    default:
                        break;
                }

                if ((DockingPlateCylinder != null) && (Presence_IO != null) && (CP_OUT_IO != null))
                {
                    //6, 8inch unlocking
                    if (Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6 ||
                       Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                    {
                        retIOvalue = IO.MonitorForIO(Presence_IO, Level, maintaintime, timeout);
                        //there is cassette
                        if (retIOvalue == 0)
                        {
                            retIOvalue = IO.MonitorForIO(CP_OUT_IO, Level, maintaintime, timeout);

                            if (retIOvalue == 0)
                            {
                                retIOvalue = DockingPlateCylinder.Retract();

                                if (retIOvalue == 0)
                                {
                                    Owner.StateTransition(new TOPFoupDockingPlateStateUnLock(Owner));
                                }
                                else
                                {
                                    // TODO : Why Extend function is called?
                                    retIOvalue = DockingPlateCylinder.Extend();
                                    Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                                }
                            }
                        }
                        //there is not cassette
                        else if (retIOvalue == -2)
                        {
                            retIOvalue = IO.MonitorForIO(Presence_IO, false, maintaintime, timeout);
                            if (retIOvalue == 0)
                            {
                                retIOvalue = IO.MonitorForIO(CP_OUT_IO, Level, maintaintime, timeout);

                                if (retIOvalue == 0)
                                {
                                    retIOvalue = DockingPlateCylinder.Retract();

                                    if (retIOvalue == 0)
                                    {
                                        Owner.StateTransition(new TOPFoupDockingPlateStateUnLock(Owner));
                                    }
                                    else
                                    {
                                        // TODO : Why Extend function is called?
                                        retIOvalue = DockingPlateCylinder.Extend();
                                        Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                                    }
                                }
                            }
                            else
                            {
                                Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                            }
                        }
                        else
                        {
                            Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                        }
                    }
                    //12 inch unlocking
                    else
                    {
                        retIOvalue = IO.MonitorForIO(Presence_IO, Level, maintaintime, timeout);
                        retIOvalue1 = IO.MonitorForIO(PosA_IO, Level, maintaintime, timeout);
                        retIOvalue2 = IO.MonitorForIO(PosB_IO, Level, maintaintime, timeout);
                        //there is cassette
                        if (retIOvalue == 0 && retIOvalue1 == 0 && retIOvalue2 == 0)
                        {
                            retIOvalue = IO.MonitorForIO(CP_OUT_IO, Level, maintaintime, timeout);

                            if (retIOvalue == 0)
                            {
                                retIOvalue = DockingPlateCylinder.Retract();

                                if (retIOvalue == 0)
                                {
                                    Owner.StateTransition(new TOPFoupDockingPlateStateUnLock(Owner));
                                }
                                else
                                {
                                    // TODO : Why Extend function is called?
                                    retIOvalue = DockingPlateCylinder.Extend();
                                    Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                                }
                            }
                        }
                        //there is not cassette
                        else if (retIOvalue == -2 && retIOvalue1 == -2 && retIOvalue2 == -2)
                        {
                            retIOvalue = IO.MonitorForIO(Presence_IO, false, maintaintime, timeout);
                            retIOvalue1 = IO.MonitorForIO(PosA_IO, false, maintaintime, timeout);
                            retIOvalue2 = IO.MonitorForIO(PosB_IO, false, maintaintime, timeout);

                            if (retIOvalue == 0 && retIOvalue1 == 0 && retIOvalue2 == 0)
                            {
                                retIOvalue = IO.MonitorForIO(CP_OUT_IO, Level, maintaintime, timeout);

                                if (retIOvalue == 0)
                                {
                                    retIOvalue = DockingPlateCylinder.Retract();

                                    if (retIOvalue == 0)
                                    {
                                        Owner.StateTransition(new TOPFoupDockingPlateStateUnLock(Owner));
                                    }
                                    else
                                    {
                                        // TODO : Why Extend function is called?
                                        //retIOvalue = DockingPlateCylinder.Extend();
                                        Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                                    }
                                }
                            }
                            else
                            {
                                Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                            }
                        }
                        else
                        {
                            Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                        }
                    }
                    //retIOvalue = IO.MonitorForIO(Presence_IO, Level, maintaintime, timeout);

                    //if (retIOvalue == 0)
                    //{
                    //    retIOvalue = DockingPlateCylinder.Retract();

                    //    if (retIOvalue == 0)
                    //    {
                    //        Owner.StateTransition(new TOPFoupDockingPlateStateUnLock(Owner));
                    //    }
                    //    else
                    //    {
                    //        // TODO : Why Extend function is called?
                    //        retIOvalue = DockingPlateCylinder.Extend();
                    //        Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                    //    }
                    //}
                    //else
                    //{
                    //    Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                    //}
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{FoupModule.DeviceParam.SubstrateSize.Value}]");
                    Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                }

                if (Owner.EnumState != DockingPlateStateEnum.ERROR)
                {
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    retval = EventCodeEnum.FOUP_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                Owner.StateTransition(new TOPFoupDockingPlateStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
    }

    public class TOPFoupDockingPlateStateLock : TOPFoupDockingPlateStateBase
    {
        public TOPFoupDockingPlateStateLock(TOPFoupDockingPlate owner) : base(owner)
        {
        }

        public override DockingPlateStateEnum GetState()
        {
            return DockingPlateStateEnum.LOCK;
        }

        public override EventCodeEnum Lock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Unlock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = UnlockFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class TOPFoupDockingPlateStateUnLock : TOPFoupDockingPlateStateBase
    {
        public TOPFoupDockingPlateStateUnLock(TOPFoupDockingPlate owner) : base(owner)
        {
        }

        public override DockingPlateStateEnum GetState()
        {
            return DockingPlateStateEnum.UNLOCK;
        }

        public override EventCodeEnum Lock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = LockFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Unlock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class TOPFoupDockingPlateStateError : TOPFoupDockingPlateStateBase
    {
        public TOPFoupDockingPlateStateError(TOPFoupDockingPlate owner) : base(owner)
        {
        }

        public override DockingPlateStateEnum GetState()
        {
            return DockingPlateStateEnum.ERROR;
        }

        public override EventCodeEnum Lock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = LockFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Unlock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = UnlockFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
