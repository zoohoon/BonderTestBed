using LogModule;
using ProberErrorCode;
using ProberInterfaces.Foup;
using System;
using ProberInterfaces;
using CylType;
using FoupModules.FoupOpener;

namespace FoupModules.Template.TOP
{
    public class TOPFoupOpener : FoupOpenerBase, ITemplateModule
    {
        #region // ITemplateModule implementation.
        public bool Initialized { get; set; } = false;
        public void DeInitModule()
        {

        }

        public TOPFoupOpener()
        {

        }
        public EventCodeEnum InitModule()
        {
            //EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            //ret = StateInit();
            //if (ret != EventCodeEnum.NONE)
            //{
            //    LoggerManager.Debug($"TOPFoupOpener.InitModule(): Init. error. Ret = {ret}");
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

        private TOPFoupOpenerStateBase _State;

        public TOPFoupOpenerStateBase State
        {
            get { return _State; }
            set { _State = value; }
        }

        public TOPFoupOpener(FoupModule module) : base(module)
        {
            StateInit();
        }
        public void StateTransition(TOPFoupOpenerStateBase state)
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

        public override FoupCassetteOpenerStateEnum GetState()
        {
            return State.GetState();
        }

        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            var IO = Module.IOManager;

            try
            {
                if ((Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6) ||
                    (Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8) ||
                    (Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                    )
                {
                    //Close
                    int ret1 = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_CLOSE, true, FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME, FoupIOGlobalVar.IO_CHECK_TIME_OUT);
                    int ret2 = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_OPEN, false, FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME, FoupIOGlobalVar.IO_CHECK_TIME_OUT);

                    if (ret1 == -2 && ret2 == -2)
                    {
                        //Open
                        int ret3 = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_CLOSE, false, FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME, FoupIOGlobalVar.IO_CHECK_TIME_OUT);
                        int ret4 = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_OPEN, true, FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME, FoupIOGlobalVar.IO_CHECK_TIME_OUT);

                        if (ret3 == 0 && ret4 == 0)
                        {
                            StateTransition(new TOPFoupOpenerStateUnlock(this));
                        }
                        else
                        {
                            StateTransition(new TOPFoupOpenerStateError(this));
                        }
                    }
                    else if (ret1 == 0 && ret2 == 0)
                    {
                        StateTransition(new TOPFoupOpenerStateLock(this));
                    }
                    else
                    {
                        StateTransition(new TOPFoupOpenerStateError(this));
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{Module.DeviceParam.SubstrateSize.Value}]");
                    StateTransition(new TOPFoupOpenerStateError(this));
                }

                if (EnumState == FoupCassetteOpenerStateEnum.ERROR)
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
                StateTransition(new TOPFoupOpenerStateError(this));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
    }
    public abstract class TOPFoupOpenerStateBase
    {
        public TOPFoupOpenerStateBase(TOPFoupOpener owner)
        {
            this.Owner = owner;
        }

        private TOPFoupOpener _Owner;
        public TOPFoupOpener Owner
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

        public abstract FoupCassetteOpenerStateEnum GetState();

        public abstract EventCodeEnum Lock();

        public abstract EventCodeEnum Unlock();

        public EventCodeEnum LockFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            int retIOvalue = -1;
            int retIOvalue2 = -1;

            try
            {
                retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_OPEN, false, IO.IOMap.Inputs.DI_FO_OPEN.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_OPEN.TimeOut.Value);
                retIOvalue2 = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_CLOSE, true, IO.IOMap.Inputs.DI_FO_CLOSE.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_CLOSE.TimeOut.Value);

                //cassette is locking
                if ((retIOvalue == 0) && (retIOvalue2 == 0))
                {
                    if ((FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6) ||
                        (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                        )
                    {
                        Owner.StateTransition(new TOPFoupOpenerStateLock(Owner));
                    }
                    else if (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                    {
                        retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_UP, true, IO.IOMap.Inputs.DI_FO_UP.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_UP.TimeOut.Value);

                        if (retIOvalue == 0)
                        {
                            retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_40_IN, true, IO.IOMap.Inputs.DI_CP_40_IN.MaintainTime.Value, IO.IOMap.Inputs.DI_CP_40_IN.TimeOut.Value);

                            if (retIOvalue == 0)
                            {
                                retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_COVER, true, IO.IOMap.Inputs.DI_COVER.MaintainTime.Value, IO.IOMap.Inputs.DI_COVER.TimeOut.Value);
                                //cover is attached on door
                                if (retIOvalue == 0)
                                {
                                    retIOvalue = IO.WriteBit(IO.IOMap.Outputs.DO_FO_VAC_AIR, false);

                                    if (retIOvalue == 0)
                                    {
                                        retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_VAC, false, IO.IOMap.Inputs.DI_FO_VAC.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_VAC.TimeOut.Value);

                                        if (retIOvalue == 0)
                                        {
                                            retIOvalue = FoupCylinderType.FoupRotator.Extend();

                                            if (retIOvalue == 0)
                                            {
                                                Owner.StateTransition(new TOPFoupOpenerStateLock(Owner));
                                            }
                                            else
                                            {
                                                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                            }
                                        }
                                        else
                                        {
                                            Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                        }
                                    }
                                    else
                                    {
                                        Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                    }
                                }
                                //cover is not attached on door
                                else if (retIOvalue == -2)
                                {
                                    retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_COVER, false, IO.IOMap.Inputs.DI_COVER.MaintainTime.Value, IO.IOMap.Inputs.DI_COVER.TimeOut.Value);

                                    if (retIOvalue == 0)
                                    {
                                        retIOvalue = IO.WriteBit(IO.IOMap.Outputs.DO_FO_VAC_AIR, false);

                                        if (retIOvalue == 0)
                                        {
                                            retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_VAC, false, IO.IOMap.Inputs.DI_FO_VAC.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_VAC.TimeOut.Value);

                                            if (retIOvalue == 0)
                                            {
                                                retIOvalue = FoupCylinderType.FoupRotator.Extend();

                                                if (retIOvalue == 0)
                                                {
                                                    Owner.StateTransition(new TOPFoupOpenerStateLock(Owner));
                                                }
                                                else
                                                {
                                                    Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                                }
                                            }
                                            else
                                            {
                                                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                            }
                                        }
                                        else
                                        {
                                            Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                        }
                                    }

                                    else
                                    {
                                        Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                    }

                                }

                                else
                                {
                                    Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                }
                            }

                            else
                            {
                                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                            }
                        }

                        else
                        {
                            Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                        }

                    }
                    else
                    {
                        Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                    }
                }
                //cassette is unlocking
                else if ((retIOvalue == -2) && (retIOvalue2 == -2))
                {
                    retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_OPEN, true, IO.IOMap.Inputs.DI_FO_OPEN.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_OPEN.TimeOut.Value);
                    retIOvalue2 = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_CLOSE, false, IO.IOMap.Inputs.DI_FO_CLOSE.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_CLOSE.TimeOut.Value);
                    // cassette is unlocking
                    if ((retIOvalue == 0) && (retIOvalue2 == 0))
                    {
                        if ((FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6) ||
                            (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                            )
                        {
                            Owner.StateTransition(new TOPFoupOpenerStateUnlock(Owner));
                        }
                        else if (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                        {
                            retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_UP, true, IO.IOMap.Inputs.DI_FO_UP.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_UP.TimeOut.Value);

                            if (retIOvalue == 0)
                            {
                                retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_40_IN, true, IO.IOMap.Inputs.DI_CP_40_IN.MaintainTime.Value, IO.IOMap.Inputs.DI_CP_40_IN.TimeOut.Value);

                                if (retIOvalue == 0)
                                {
                                    retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_COVER, true, IO.IOMap.Inputs.DI_COVER.MaintainTime.Value, IO.IOMap.Inputs.DI_COVER.TimeOut.Value);
                                    //cover is attached on door
                                    if (retIOvalue == 0)
                                    {
                                        retIOvalue = IO.WriteBit(IO.IOMap.Outputs.DO_FO_VAC_AIR, false);

                                        if (retIOvalue == 0)
                                        {
                                            retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_VAC, false, IO.IOMap.Inputs.DI_FO_VAC.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_VAC.TimeOut.Value);

                                            if (retIOvalue == 0)
                                            {
                                                retIOvalue = FoupCylinderType.FoupRotator.Extend();

                                                if (retIOvalue == 0)
                                                {
                                                    Owner.StateTransition(new TOPFoupOpenerStateLock(Owner));
                                                }
                                                else
                                                {
                                                    Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                                }
                                            }
                                            else
                                            {
                                                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                            }
                                        }
                                        else
                                        {
                                            Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                        }
                                    }
                                    //cover is not attached on door
                                    else if (retIOvalue == -2)
                                    {
                                        retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_COVER, false, IO.IOMap.Inputs.DI_COVER.MaintainTime.Value, IO.IOMap.Inputs.DI_COVER.TimeOut.Value);

                                        if (retIOvalue == 0)
                                        {
                                            retIOvalue = IO.WriteBit(IO.IOMap.Outputs.DO_FO_VAC_AIR, false);

                                            if (retIOvalue == 0)
                                            {
                                                retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_VAC, false, IO.IOMap.Inputs.DI_FO_VAC.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_VAC.TimeOut.Value);

                                                if (retIOvalue == 0)
                                                {
                                                    retIOvalue = FoupCylinderType.FoupRotator.Extend();

                                                    if (retIOvalue == 0)
                                                    {
                                                        Owner.StateTransition(new TOPFoupOpenerStateLock(Owner));
                                                    }
                                                    else
                                                    {
                                                        Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                                    }
                                                }
                                                else
                                                {
                                                    Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                                }
                                            }
                                            else
                                            {
                                                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                            }
                                        }
                                        else
                                        {
                                            Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                        }
                                    }
                                    else
                                    {
                                        Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                    }
                                }

                                else
                                {
                                    Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                }

                            }
                            else
                            {
                                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                            }
                        }
                        else
                        {
                            Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                        }
                    }
                    else
                    {
                        Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                    }
                }
                else
                {
                    Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                }

                if (Owner.EnumState == FoupCassetteOpenerStateEnum.ERROR)
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
                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }

        public EventCodeEnum UnlockFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            int retIOvalue = -1;
            int retIOvalue2 = -1;

            try
            {
                retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_OPEN, true, 300, 3000);
                retIOvalue2 = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_CLOSE, false, 300, 3000);

                //cassette is unlocking
                if ((retIOvalue == 0) && (retIOvalue2 == 0))
                {
                    if ((FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6) ||
                        (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                        )
                    {
                        Owner.StateTransition(new TOPFoupOpenerStateUnlock(Owner));
                    }
                    else if (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                    {
                        retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_UP, true, IO.IOMap.Inputs.DI_FO_UP.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_UP.TimeOut.Value);
                        if (retIOvalue == 0)
                        {
                            retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_40_IN, true, IO.IOMap.Inputs.DI_CP_40_IN.MaintainTime.Value, IO.IOMap.Inputs.DI_CP_40_IN.TimeOut.Value);
                            if (retIOvalue == 0)
                            {
                                retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_COVER, true, IO.IOMap.Inputs.DI_COVER.MaintainTime.Value, IO.IOMap.Inputs.DI_COVER.TimeOut.Value);
                                //cover is attached on door
                                if (retIOvalue == 0)
                                {
                                    retIOvalue = IO.WriteBit(IO.IOMap.Outputs.DO_FO_VAC_AIR, true);

                                    if (retIOvalue == 0)
                                    {
                                        retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_VAC, true, IO.IOMap.Inputs.DI_FO_VAC.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_VAC.TimeOut.Value);

                                        if (retIOvalue == 0)
                                        {
                                            retIOvalue = FoupCylinderType.FoupRotator.Retract();

                                            if (retIOvalue == 0)
                                            {
                                                Owner.StateTransition(new TOPFoupOpenerStateUnlock(Owner));
                                            }
                                            else
                                            {
                                                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                            }
                                        }
                                        else
                                        {
                                            Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                        }
                                    }
                                    else
                                    {
                                        Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                    }
                                }
                                //cover is not attached on door
                                else if (retIOvalue == -2)
                                {
                                    retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_COVER, false, IO.IOMap.Inputs.DI_COVER.MaintainTime.Value, IO.IOMap.Inputs.DI_COVER.TimeOut.Value);

                                    if (retIOvalue == 0)
                                    {
                                        retIOvalue = IO.WriteBit(IO.IOMap.Outputs.DO_FO_VAC_AIR, false);

                                        if (retIOvalue == 0)
                                        {
                                            retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_VAC, false, IO.IOMap.Inputs.DI_FO_VAC.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_VAC.TimeOut.Value);

                                            if (retIOvalue == 0)
                                            {
                                                retIOvalue = FoupCylinderType.FoupRotator.Extend();

                                                if (retIOvalue == 0)
                                                {
                                                    Owner.StateTransition(new TOPFoupOpenerStateUnlock(Owner));
                                                }
                                                else
                                                {
                                                    Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                                }
                                            }
                                            else
                                            {
                                                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                            }
                                        }
                                        else
                                        {
                                            Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                        }
                                    }
                                    else
                                    {
                                        Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                    }
                                }
                                else
                                {
                                    Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                }
                            }
                            else
                            {
                                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                            }
                        }
                        else
                        {
                            Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                        }
                    }
                    else
                    {
                        Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                    }
                }
                //cassette is locking
                else if ((retIOvalue == -2) && (retIOvalue2 == -2))
                {
                    retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_OPEN, false, IO.IOMap.Inputs.DI_FO_OPEN.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_OPEN.TimeOut.Value);
                    retIOvalue2 = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_CLOSE, true, IO.IOMap.Inputs.DI_FO_CLOSE.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_CLOSE.TimeOut.Value);
                    //cassette is locking
                    if ((retIOvalue == 0) && (retIOvalue2 == 0))
                    {
                        if ((FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6) ||
                            (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                            )
                        {
                            Owner.StateTransition(new TOPFoupOpenerStateUnlock(Owner));
                        }
                        else if (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                        {
                            retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_UP, true, IO.IOMap.Inputs.DI_FO_UP.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_UP.TimeOut.Value);

                            if (retIOvalue == 0)
                            {
                                retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_40_IN, true, IO.IOMap.Inputs.DI_CP_40_IN.MaintainTime.Value, IO.IOMap.Inputs.DI_CP_40_IN.TimeOut.Value);

                                if (retIOvalue == 0)
                                {
                                    retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_COVER, true, IO.IOMap.Inputs.DI_COVER.MaintainTime.Value, IO.IOMap.Inputs.DI_COVER.TimeOut.Value);
                                    //cover is attached on door
                                    if (retIOvalue == 0)
                                    {
                                        retIOvalue = IO.WriteBit(IO.IOMap.Outputs.DO_FO_VAC_AIR, true);

                                        if (retIOvalue == 0)
                                        {
                                            retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_VAC, true, IO.IOMap.Inputs.DI_FO_VAC.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_VAC.TimeOut.Value);

                                            if (retIOvalue == 0)
                                            {
                                                retIOvalue = FoupCylinderType.FoupRotator.Retract();

                                                if (retIOvalue == 0)
                                                {
                                                    Owner.StateTransition(new TOPFoupOpenerStateUnlock(Owner));
                                                }
                                                else
                                                {
                                                    Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                                }
                                            }
                                            else
                                            {
                                                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                            }
                                        }
                                        else
                                        {
                                            Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                        }
                                    }
                                    //cover is not attached on door
                                    else if (retIOvalue == -2)
                                    {
                                        retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_COVER, false, IO.IOMap.Inputs.DI_COVER.MaintainTime.Value, IO.IOMap.Inputs.DI_COVER.TimeOut.Value);

                                        if (retIOvalue == 0)
                                        {
                                            retIOvalue = IO.WriteBit(IO.IOMap.Outputs.DO_FO_VAC_AIR, false);

                                            if (retIOvalue == 0)
                                            {
                                                retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_VAC, false, IO.IOMap.Inputs.DI_FO_VAC.MaintainTime.Value, IO.IOMap.Inputs.DI_FO_VAC.TimeOut.Value);

                                                if (retIOvalue == 0)
                                                {
                                                    retIOvalue = FoupCylinderType.FoupRotator.Extend();

                                                    if (retIOvalue == 0)
                                                    {
                                                        Owner.StateTransition(new TOPFoupOpenerStateUnlock(Owner));
                                                    }
                                                    else
                                                    {
                                                        Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                                    }
                                                }
                                                else
                                                {
                                                    Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                                }
                                            }
                                            else
                                            {
                                                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                            }
                                        }
                                        else
                                        {
                                            Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                        }
                                    }
                                    else
                                    {
                                        Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                    }
                                }
                                else
                                {
                                    Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                                }

                            }
                            else
                            {
                                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                            }

                        }
                        else
                        {
                            Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                        }
                    }
                    else
                    {
                        Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                    }
                }
                else
                {
                    Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                }

                if (Owner.EnumState == FoupCassetteOpenerStateEnum.ERROR)
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
                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
    }

    public class TOPFoupOpenerStateLock : TOPFoupOpenerStateBase
    {
        public TOPFoupOpenerStateLock(TOPFoupOpener owner) : base(owner)
        {
        }

        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.LOCK;
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
                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
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
                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class TOPFoupOpenerStateUnlock : TOPFoupOpenerStateBase
    {
        public TOPFoupOpenerStateUnlock(TOPFoupOpener owner) : base(owner)
        {
        }

        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.UNLOCK;
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
                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
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
                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class TOPFoupOpenerStateError : TOPFoupOpenerStateBase
    {
        public TOPFoupOpenerStateError(TOPFoupOpener owner) : base(owner)
        {
        }

        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.ERROR;
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
                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
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
                Owner.StateTransition(new TOPFoupOpenerStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
