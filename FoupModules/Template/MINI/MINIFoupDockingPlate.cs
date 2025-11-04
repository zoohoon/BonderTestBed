using CylType;
using FoupModules.DockingPlate;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System;

namespace FoupModules.Template.MINI
{
    public class MINIFoupDockingPlate : FoupDockingPlateBase, ITemplateModule
    {
        private MINIFoupDockingPlateStateBase _State;

        public MINIFoupDockingPlateStateBase State
        {
            get { return _State; }
            set { _State = value; }
        }

        public MINIFoupDockingPlate()
        {

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
        public MINIFoupDockingPlate(FoupModule module) : base(module)
        {
            StateInit();
        }

        public void StateTransition(MINIFoupDockingPlateStateBase state)
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
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    switch (EnumState)
                    {
                        case DockingPlateStateEnum.LOCK:
                            State = new MINIFoupDockingPlateStateLock(this);
                            break;
                        case DockingPlateStateEnum.UNLOCK:
                            State = new MINIFoupDockingPlateStateUnLock(this);
                            break;
                        case DockingPlateStateEnum.ERROR:
                            State = new MINIFoupDockingPlateStateError(this);
                            break;
                        default:
                            State = new MINIFoupDockingPlateStateError(this);
                            break;
                    }

                    return EventCodeEnum.NONE;
                }

                if (Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6)
                {
                    // TODO

                    bool PLACEMENT_6Inch_value;
                    bool PRESENCE2_value;
                    bool PRESENCE3_value;
                    bool NPLACEMENT_6_8Inch_value;

                    int plcret = IO.ReadBit(IO.IOMap.Inputs.DI_C6IN_PLACEMENT, out PLACEMENT_6Inch_value);
                    int pre2ret = IO.ReadBit(IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2, out PRESENCE2_value);
                    int pre3ret = IO.ReadBit(IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE3, out PRESENCE3_value);
                    int nplcret = IO.ReadBit(IO.IOMap.Inputs.DI_C6IN_C8IN_NPLACEMENT, out NPLACEMENT_6_8Inch_value);

                    if (plcret != 0)
                    {
                        State = new MINIFoupDockingPlateStateError(this);
                    }
                    else
                    {
                        if (PLACEMENT_6Inch_value == true)
                        {
                            if (PRESENCE2_value != true || PRESENCE3_value != true)
                            {
                                retValue = FoupCylinderType.FoupDockingPlate6.Retract();

                                if (NPLACEMENT_6_8Inch_value == true)
                                {
                                    State = new MINIFoupDockingPlateStateUnLock(this);
                                }
                                else
                                {
                                    State = new MINIFoupDockingPlateStateError(this);
                                }
                            }
                            else
                            {
                                // TODO
                                retValue = FoupCylinderType.FoupDockingPlate6.Extend();
                                State = new MINIFoupDockingPlateStateLock(this);
                            }
                        }
                        else
                        {
                            retValue = FoupCylinderType.FoupDockingPlate6.Retract();

                            if (NPLACEMENT_6_8Inch_value == true)
                            {
                                State = new MINIFoupDockingPlateStateUnLock(this);
                            }
                            else
                            {
                                State = new MINIFoupDockingPlateStateError(this);
                            }
                        }
                    }
                }
                else if (Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                {
                    bool PLACEMENT_8Inch_value;
                    bool PRESENCE2_value;
                    bool PRESENCE3_value;
                    bool NPLACEMENT_6_8Inch_value;

                    int ret = IO.ReadBit(IO.IOMap.Inputs.DI_C8IN_PLACEMENT, out PLACEMENT_8Inch_value);
                    int pre2ret = IO.ReadBit(IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2, out PRESENCE2_value);
                    int pre3ret = IO.ReadBit(IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE3, out PRESENCE3_value);
                    int nplcret = IO.ReadBit(IO.IOMap.Inputs.DI_C6IN_C8IN_NPLACEMENT, out NPLACEMENT_6_8Inch_value);

                    if (ret != 0)
                    {
                        State = new MINIFoupDockingPlateStateError(this);
                    }
                    else
                    {
                        // LOCK 됐을 때
                        if (PLACEMENT_8Inch_value == true)
                        {
                            // 카세트가 놓여있지 않으면
                            if (PRESENCE2_value != true || PRESENCE3_value != true)
                            {
                                retValue = FoupCylinderType.FoupDockingPlate8.Retract();
                                nplcret = IO.ReadBit(IO.IOMap.Inputs.DI_C6IN_C8IN_NPLACEMENT, out NPLACEMENT_6_8Inch_value);
                                if (NPLACEMENT_6_8Inch_value == true)
                                {
                                    State = new MINIFoupDockingPlateStateUnLock(this);
                                }
                                else
                                {
                                    State = new MINIFoupDockingPlateStateError(this);
                                }
                            }
                            // 카세트가 놓여있으면
                            else
                            {
                                // TODO
                                retValue = FoupCylinderType.FoupDockingPlate8.Extend();
                                State = new MINIFoupDockingPlateStateLock(this);
                            }
                        }
                        else
                        {
                            retValue = FoupCylinderType.FoupDockingPlate8.Retract();
                            nplcret = IO.ReadBit(IO.IOMap.Inputs.DI_C6IN_C8IN_NPLACEMENT, out NPLACEMENT_6_8Inch_value);
                            // Unlock 됐을 때
                            if (NPLACEMENT_6_8Inch_value == true)
                            {
                                State = new MINIFoupDockingPlateStateUnLock(this);
                            }
                            else
                            {
                                State = new MINIFoupDockingPlateStateError(this);
                            }
                        }
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{Module.DeviceParam.SubstrateSize.Value}]");
                    StateTransition(new MINIFoupDockingPlateStateError(this));
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
                StateTransition(new MINIFoupDockingPlateStateError(this));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
    }
    public abstract class MINIFoupDockingPlateStateBase
    {
        public MINIFoupDockingPlateStateBase(MINIFoupDockingPlate owner)
        {
            this.Owner = owner;
        }

        private MINIFoupDockingPlate _Owner;
        public MINIFoupDockingPlate Owner
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
        public abstract EventCodeEnum RecoveryUnlock();
        public EventCodeEnum LockFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            int retIOvalue = -1;
            //int retIOvalue1 = -1;
            //int retIOvalue2 = -1;
            var IO = Owner.Module.IOManager;
            FoupCylinderType DockingPlateCylinder = null;
            IOPortDescripter<bool> Presence2_IO = null;
            IOPortDescripter<bool> Presence3_IO = null;
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
                        Presence2_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2;
                        Presence3_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE3;
                        Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = FoupIOGlobalVar.IO_CHECK_TIME_OUT;
                        break;
                    case SubstrateSizeEnum.INCH8:
                        DockingPlateCylinder = FoupCylinderType.FoupDockingPlate8;
                        Presence2_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2;
                        Presence3_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE3;
                        Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = 3000;
                        break;
                    case SubstrateSizeEnum.CUSTOM:
                        break;
                    default:
                        break;
                }

                if ((DockingPlateCylinder != null) && (Presence2_IO != null) && (Presence3_IO != null))
                {
                    //6, 8inch locking
                    if (Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6 ||
                       Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                    {
                        retIOvalue = IO.MonitorForIO(Presence2_IO, Level, maintaintime, timeout);
                        //there is cassette
                        if (retIOvalue == 0)
                        {
                            retIOvalue = IO.MonitorForIO(Presence3_IO, Level, maintaintime, timeout);

                            if (retIOvalue == 0)
                            {
                                retIOvalue = DockingPlateCylinder.Extend();

                                if (retIOvalue == 0)
                                {
                                    Owner.StateTransition(new MINIFoupDockingPlateStateLock(Owner));
                                }
                                else
                                {
                                    // TODO : Why Retract function is called?
                                    //retIOvalue = DockingPlateCylinder.Retract();
                                    Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                                }
                            }
                            else
                            {
                                Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));

                            }
                            //else if (retIOvalue == -2)
                            //{
                            //    retIOvalue = IO.MonitorForIO(Presence3_IO, !Level, maintaintime, timeout);
                            //    if (retIOvalue == 0)
                            //    {
                            //        Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                            //    }

                            //}
                        }
                        //there is not cassette
                        //else if (retIOvalue == -2)
                        //{
                        //    retIOvalue = IO.MonitorForIO(Presence2_IO, !Level, maintaintime, timeout);

                        //    if (retIOvalue == 0)
                        //    {
                        //        retIOvalue = IO.MonitorForIO(Presence3_IO, Level, maintaintime, timeout);

                        //        if (retIOvalue == 0)
                        //        {
                        //            Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                        //        }
                        //        else
                        //        {
                        //            retIOvalue = IO.MonitorForIO(Presence3_IO, !Level, maintaintime, timeout);
                        //            if (retIOvalue == 0)
                        //            {
                        //                retIOvalue = DockingPlateCylinder.Retract();
                        //                if (retIOvalue == 0)
                        //                {
                        //                    Owner.StateTransition(new MINIFoupDockingPlateStateUnLock(Owner));
                        //                }
                        //                else
                        //                {
                        //                    Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                        //                }
                        //            }
                        //            else
                        //            {
                        //                Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {
                        //        retIOvalue = IO.MonitorForIO(Presence3_IO, !Level, maintaintime, timeout);
                        //        if (retIOvalue == 0)
                        //        {

                        //        }
                        //        Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                        //    }
                        //}
                        else
                        {
                            Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                        }
                    }
                    //12 inch locking
                    else
                    {
                        LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{FoupModule.DeviceParam.SubstrateSize.Value}]");
                        Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{FoupModule.DeviceParam.SubstrateSize.Value}]");
                    Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
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
                Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
        public EventCodeEnum RecoveryUnlockFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            int retIOvalue = -1;
            //int retIOvalue1 = -1;
            //int retIOvalue2 = -1;
            var IO = Owner.Module.IOManager;
            FoupCylinderType DockingPlateCylinder = null;
            IOPortDescripter<bool> Presence2_IO = null;
            IOPortDescripter<bool> Presence3_IO = null;
            //bool Level = true;
            long maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
            long timeout = FoupIOGlobalVar.IO_CHECK_TIME_OUT;

            try
            {
                if(Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return EventCodeEnum.NONE;
                }

                switch (FoupModule.DeviceParam.SubstrateSize.Value)
                {
                    case SubstrateSizeEnum.UNDEFINED:
                        break;
                    case SubstrateSizeEnum.INCH6:
                        DockingPlateCylinder = FoupCylinderType.FoupDockingPlate6;
                        Presence2_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2;
                        Presence3_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE3;
                        //Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = FoupIOGlobalVar.IO_CHECK_TIME_OUT;
                        break;
                    case SubstrateSizeEnum.INCH8:
                        DockingPlateCylinder = FoupCylinderType.FoupDockingPlate8;
                        Presence2_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2;
                        Presence3_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE3;
                        //Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = 3000;
                        break;
                    case SubstrateSizeEnum.CUSTOM:
                        break;
                    default:
                        break;
                }

                if ((DockingPlateCylinder != null) && (Presence2_IO != null) && (Presence3_IO != null))
                {
                    //6, 8inch unlocking
                    if (Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6 ||
                       Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                    {
                        retIOvalue = DockingPlateCylinder.Retract();

                        if (retIOvalue == 0)
                        {
                            Owner.StateTransition(new MINIFoupDockingPlateStateUnLock(Owner));
                        }
                        else
                        {
                            // TODO : Why Extend function is called?
                            //retIOvalue = DockingPlateCylinder.Extend();
                            Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                        }

                    }
                    //12 inch unlocking
                    else
                    {
                        LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{FoupModule.DeviceParam.SubstrateSize.Value}]");
                        Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                    }

                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{FoupModule.DeviceParam.SubstrateSize.Value}]");
                    Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
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
                Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
        public EventCodeEnum UnlockFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            int retIOvalue = -1;
            //int retIOvalue1 = -1;
            //int retIOvalue2 = -1;
            var IO = Owner.Module.IOManager;
            FoupCylinderType DockingPlateCylinder = null;
            IOPortDescripter<bool> Presence2_IO = null;
            IOPortDescripter<bool> Presence3_IO = null;
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
                        Presence2_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2;
                        Presence3_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE3;
                        Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = FoupIOGlobalVar.IO_CHECK_TIME_OUT;
                        break;
                    case SubstrateSizeEnum.INCH8:
                        DockingPlateCylinder = FoupCylinderType.FoupDockingPlate8;
                        Presence2_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2;
                        Presence3_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE3;
                        Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = 3000;
                        break;
                    case SubstrateSizeEnum.CUSTOM:
                        break;
                    default:
                        break;
                }

                if ((DockingPlateCylinder != null) && (Presence2_IO != null) && (Presence3_IO != null))
                {
                    //6, 8inch unlocking
                    if (Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6 ||
                       Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                    {
                        retIOvalue = IO.MonitorForIO(Presence2_IO, Level, maintaintime, timeout);
                        //there is cassette
                        if (retIOvalue == 0)
                        {
                            retIOvalue = IO.MonitorForIO(Presence3_IO, Level, maintaintime, timeout);

                            if (retIOvalue == 0)
                            {
                                retIOvalue = DockingPlateCylinder.Retract();

                                if (retIOvalue == 0)
                                {
                                    Owner.StateTransition(new MINIFoupDockingPlateStateUnLock(Owner));
                                }
                                else
                                {
                                    // TODO : Why Extend function is called?
                                    //retIOvalue = DockingPlateCylinder.Extend();
                                    Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                                }
                            }
                            //else if(retIOvalue == -2)
                            //{
                            //    retIOvalue = IO.MonitorForIO(Presence3_IO, !Level, maintaintime, timeout);
                            //    if (retIOvalue == 0)
                            //    {
                            //        Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                            //    }
                            //}
                            else
                            {
                                Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                            }
                        }
                        else if (retIOvalue == -2)
                        {
                            retIOvalue = DockingPlateCylinder.Retract();

                            if (retIOvalue == 0)
                            {
                                Owner.StateTransition(new MINIFoupDockingPlateStateUnLock(Owner));
                            }
                            else
                            {
                                // TODO : Why Extend function is called?
                                //retIOvalue = DockingPlateCylinder.Extend();
                                Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                            }
                        }
                        else
                        {
                            Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                        }
                    }
                    //12 inch unlocking
                    else
                    {
                        LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{FoupModule.DeviceParam.SubstrateSize.Value}]");
                        Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                    }

                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{FoupModule.DeviceParam.SubstrateSize.Value}]");
                    Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
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
                Owner.StateTransition(new MINIFoupDockingPlateStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
    }

    public class MINIFoupDockingPlateStateLock : MINIFoupDockingPlateStateBase
    {
        public MINIFoupDockingPlateStateLock(MINIFoupDockingPlate owner) : base(owner)
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

        public override EventCodeEnum RecoveryUnlock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RecoveryUnlockFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class MINIFoupDockingPlateStateUnLock : MINIFoupDockingPlateStateBase
    {
        public MINIFoupDockingPlateStateUnLock(MINIFoupDockingPlate owner) : base(owner)
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
        public override EventCodeEnum RecoveryUnlock()
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

    public class MINIFoupDockingPlateStateError : MINIFoupDockingPlateStateBase
    {
        public MINIFoupDockingPlateStateError(MINIFoupDockingPlate owner) : base(owner)
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
        public override EventCodeEnum RecoveryUnlock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = RecoveryUnlockFunc();
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
