using FoupModules.FoupCover;
using LogModule;
using ProberErrorCode;
using ProberInterfaces.Foup;
using System;
using ProberInterfaces;
using CylType;

namespace FoupModules.Template.FLAT
{
    public class FLATFoupCover : FoupCoverBase, ITemplateModule
    {

        private FLATFoupCoverStateBase _StateObj;

        public FLATFoupCoverStateBase StateObj
        {
            get { return _StateObj; }
            set { _StateObj = value; }
        }
        #region // ITemplateModule implementation.
        public bool Initialized { get; set; } = false;
        public void DeInitModule()
        {

        }

        public FLATFoupCover()
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

        public FLATFoupCover(FoupModule module) : base(module)
        {
            StateInit();
        }
        public void StateTransition(FLATFoupCoverStateBase state)
        {
            try
            {
                _StateObj = state;

                //this.EnumState = _StateObj.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Close()
        {
            return StateObj.Up();
        }

        public override EventCodeEnum Open()
        {
            return StateObj.Down();
        }

        public override FoupCoverStateEnum GetState()
        {
            return StateObj.GetState();
        }

        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            var IO = Module.IOManager;

            try
            {
                if (Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6)
                {
                    // TODO
                }
                else if (Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                {
                    bool FOUP, FODOWN;
                    int ret = IO.ReadBit(IO.IOMap.Inputs.DI_FO_UP, out FOUP);
                    int ret2 = IO.ReadBit(IO.IOMap.Inputs.DI_FO_DOWN, out FODOWN);

                    if (ret != 0 || ret2 != 0)
                    {
                        StateTransition(new FLATFoupCoverStateError(this));
                    }
                    else
                    {
                        if (FOUP == true && FODOWN == false)
                        {
                            StateTransition(new FLATFoupCoverStateUp(this));
                        }
                        else if (FOUP == false && FODOWN == true)
                        {
                            StateTransition(new FLATFoupCoverStateDown(this));
                        }
                        else
                        {
                            StateTransition(new FLATFoupCoverStateError(this));
                        }
                    }
                }
                else if (Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                {
                    int ret1 = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_DOWN, false, 300, 1000);
                    int ret2 = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_UP, true, 300, 1000);

                    if (ret1 == -2 && ret2 == -2)
                    {
                        int ret3 = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_DOWN, true, 300, 1000);
                        int ret4 = IO.MonitorForIO(IO.IOMap.Inputs.DI_FO_UP, false, 300, 1000);

                        if (ret3 == 0 && ret4 == 0)
                        {
                            StateTransition(new FLATFoupCoverStateDown(this));
                        }
                        else
                        {
                            StateTransition(new FLATFoupCoverStateError(this));
                        }
                    }
                    else if (ret1 == 0 && ret2 == 0)
                    {
                        StateTransition(new FLATFoupCoverStateUp(this));
                    }
                    else
                    {
                        StateTransition(new FLATFoupCoverStateError(this));
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{Module.DeviceParam.SubstrateSize.Value}]");
                    StateTransition(new FLATFoupCoverStateError(this));
                }

                if (EnumState == FoupCoverStateEnum.ERROR)
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
                StateTransition(new FLATFoupCoverStateError(this));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
        public override EventCodeEnum CheckState()
        {
            EventCodeEnum EventCodeEnum = EventCodeEnum.NONE;

            return EventCodeEnum;
        }
    }
    public abstract class FLATFoupCoverStateBase
    {
        public FLATFoupCoverStateBase(FLATFoupCover owner)
        {
            this.Owner = owner;
        }

        private FLATFoupCover _Owner;
        public FLATFoupCover Owner
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

        public abstract FoupCoverStateEnum GetState();

        public abstract EventCodeEnum Up();

        public abstract EventCodeEnum Down();

        public EventCodeEnum UpFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            int retIOvalue = -1;

            try
            {
                if (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6 ||
                   FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8 ||
                   FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                {
                    retIOvalue = Owner.Module.IOManager.MonitorForIO(Owner.Module.IOManager.IOMap.Inputs.DI_CP_40_OUT, true, Owner.Module.IOManager.IOMap.Inputs.DI_CP_40_OUT.MaintainTime.Value, Owner.Module.IOManager.IOMap.Inputs.DI_CP_40_OUT.TimeOut.Value);

                    if (retIOvalue == 0)
                    {
                        retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_WAFER_OUT, true, IO.IOMap.Inputs.DI_WAFER_OUT.MaintainTime.Value, IO.IOMap.Inputs.DI_WAFER_OUT.TimeOut.Value);

                        //Error Check Wafer Out
                        if (retIOvalue != 0)
                        {
                            Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                        }
                        else
                        {
                            retval = EventCodeEnum.NONE;
                        }
                    }
                    else
                    {
                        Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                    }
                }
                //if (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6)
                //{

                //}
                //else if (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                //{
                //    retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_WAFER_OUT, true, 500, 10000);

                //    //Error Check Wafer Out
                //    if (retIOvalue != 0)
                //    {
                //        Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                //    }
                //}
                //else if (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                //{
                //    retIOvalue = Owner.Module.IOManager.MonitorForIO(Owner.Module.IOManager.IOMap.Inputs.DI_CP_40_OUT, true, 10, 300);

                //    if (retIOvalue == 0)
                //    {
                //        retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_WAFER_OUT, true, 500, 10000);

                //        //Error Check Wafer Out
                //        if (retIOvalue != 0)
                //        {
                //            Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                //        }
                //    }
                //    else
                //    {
                //        Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                //    }
                //}
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{FoupModule.DeviceParam.SubstrateSize.Value}]");
                }

                // Success IO Check
                if (retval == EventCodeEnum.NONE)
                {
                    retIOvalue = FoupCylinderType.FoupCover.Extend();

                    if (retIOvalue == 0)
                    {
                        Owner.StateTransition(new FLATFoupCoverStateUp(Owner));
                        retval = EventCodeEnum.NONE;
                    }
                    else
                    {
                        Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                        retval = EventCodeEnum.FOUP_ERROR;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }

        public EventCodeEnum DownFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            int retIOvalue = -1;

            try
            {
                if (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6 ||
                   FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8 ||
                   FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                {
                    retIOvalue = Owner.Module.IOManager.MonitorForIO(Owner.Module.IOManager.IOMap.Inputs.DI_CP_40_OUT, true, 500, 10000);

                    if (retIOvalue == 0)
                    {
                        retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_WAFER_OUT, true, IO.IOMap.Inputs.DI_WAFER_OUT.MaintainTime.Value, IO.IOMap.Inputs.DI_WAFER_OUT.TimeOut.Value);

                        //Error Check Wafer Out
                        if (retIOvalue != 0)
                        {
                            Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                        }
                    }
                    else
                    {
                        Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                    }
                }
                //if (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6)
                //{
                //    // TODO
                //}
                //else if (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                //{
                //    retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_WAFER_OUT, true, 500, 10000);

                //    //Error Check Wafer Out
                //    if (retIOvalue != 0)
                //    {
                //        Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                //    }
                //}
                //else if (FoupModule.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                //{
                //    retIOvalue = Owner.Module.IOManager.MonitorForIO(Owner.Module.IOManager.IOMap.Inputs.DI_CP_40_OUT, true, 500, 10000);

                //    if (retIOvalue == 0)
                //    {
                //        retIOvalue = IO.MonitorForIO(IO.IOMap.Inputs.DI_WAFER_OUT, true, 500, 10000);

                //        //Error Check Wafer Out
                //        if (retIOvalue != 0)
                //        {
                //            Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                //        }
                //    }
                //    else
                //    {
                //        Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                //    }
                //}
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{FoupModule.DeviceParam.SubstrateSize.Value}]");
                }

                // Success IO Check
                if (Owner.EnumState != FoupCoverStateEnum.ERROR)
                {
                    retIOvalue = FoupCylinderType.FoupCover.Retract();

                    if (retIOvalue == 0)
                    {
                        Owner.StateTransition(new FLATFoupCoverStateDown(Owner));
                    }
                    else
                    {
                        Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                    }

                    if (Owner.EnumState != FoupCoverStateEnum.ERROR)
                    {
                        retval = EventCodeEnum.NONE;
                    }
                    else
                    {
                        retval = EventCodeEnum.FOUP_ERROR;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
    }

    public class FLATFoupCoverStateUp : FLATFoupCoverStateBase
    {
        public FLATFoupCoverStateUp(FLATFoupCover owner) : base(owner)
        {
        }

        public override FoupCoverStateEnum GetState()
        {
            return FoupCoverStateEnum.CLOSE;
        }

        public override EventCodeEnum Up()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Down()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = DownFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class FLATFoupCoverStateDown : FLATFoupCoverStateBase
    {
        public FLATFoupCoverStateDown(FLATFoupCover owner) : base(owner)
        {
        }

        public override FoupCoverStateEnum GetState()
        {
            return FoupCoverStateEnum.OPEN;
        }

        public override EventCodeEnum Up()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = UpFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Down()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class FLATFoupCoverStateError : FLATFoupCoverStateBase
    {
        public FLATFoupCoverStateError(FLATFoupCover owner) : base(owner)
        {
        }

        public override FoupCoverStateEnum GetState()
        {
            return FoupCoverStateEnum.ERROR;
        }

        public override EventCodeEnum Up()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = UpFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Down()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = DownFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new FLATFoupCoverStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
