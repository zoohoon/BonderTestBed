using FoupModules.FoupTilt;
using LogModule;
using ProberErrorCode;
using ProberInterfaces.Foup;
using System;
using ProberInterfaces;
using CylType;
using ECATIO;

namespace FoupModules.Template.MINI
{
    public class MINIFoupTilt : FoupTiltBase, ITemplateModule
    {
        public override TiltStateEnum State => StateObj.GetState();

        private MINIFoupTiltStateBase _StateObj;

        public MINIFoupTiltStateBase StateObj
        {
            get { return _StateObj; }
            set { _StateObj = value; }
        }
        #region // ITemplateModule implementation.
        public bool Initialized { get; set; } = false;
        public void DeInitModule()
        {

        }

        public MINIFoupTilt() : base()
        {

        }
        public EventCodeEnum InitModule()
        {
            //EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            //ret = StateInit();
            //if (ret != EventCodeEnum.NONE)
            //{
            //    LoggerManager.Debug($"GPFoupTilt12Inch.InitModule(): Init. error. Ret = {ret}");
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

        public MINIFoupTilt(FoupModule module) : base(module)
        {
            StateInit();
        }

        public void StateTransition(MINIFoupTiltStateBase state)
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

        public override EventCodeEnum Up()
        {
            return StateObj.Up();
        }

        public override EventCodeEnum Down()
        {
            return StateObj.Down();
        }

        public override TiltStateEnum GetState()
        {
            return StateObj.GetState();
        }

        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal;

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    switch (EnumState)
                    {
                        case TiltStateEnum.UP:
                            StateObj = new MINIFoupTiltStateUp(this);
                            break;
                        case TiltStateEnum.DOWN:
                            StateObj = new MINIFoupTiltStateDown(this);
                            break;
                        case TiltStateEnum.IDLE:
                            StateObj = new MINIFoupTiltStateError(this);
                            break;
                        case TiltStateEnum.ERROR:
                            StateObj = new MINIFoupTiltStateError(this);
                            break;
                        default:
                            break;
                    }

                    return EventCodeEnum.NONE;
                }

                bool cstdown = false;
                bool cstup = false;
                int downret = FoupIOManager.ReadBit(FoupIOManager.IOMap.Inputs.DI_CSTT_DOWN, out cstdown);
                int upret = FoupIOManager.ReadBit(FoupIOManager.IOMap.Inputs.DI_CSTT_UP, out cstup);
                if (downret == 0 && upret == 0)
                {
                    if(cstup == true && cstdown ==false)
                    {
                        StateObj = new MINIFoupTiltStateUp(this);
                        retVal = EventCodeEnum.NONE;

                    }
                    else if(cstup == false && cstdown == true)
                    {
                        StateObj = new MINIFoupTiltStateDown(this);
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        StateObj = new MINIFoupTiltStateError(this);
                        retVal = EventCodeEnum.FOUP_ERROR;
                    }
                }
                else
                {
                    StateObj = new MINIFoupTiltStateError(this);
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
                //if (ret1 == 0)
                //{
                //    StateObj = new MINIFoupTiltStateUp(this);
                //    retVal = EventCodeEnum.NONE;
                //}
                //else if (ret1 != 0)
                //{
                //    ret1 = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CSTT_DOWN, false, FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME, FoupIOGlobalVar.IO_CHECK_TIME_OUT);
                //    if (ret1 == 0)
                //    {
                //        StateObj = new MINIFoupTiltStateDown(this);
                //        retVal = EventCodeEnum.NONE;
                //    }
                //    else
                //    {
                //        StateObj = new MINIFoupTiltStateError(this);
                //        return EventCodeEnum.FOUP_ERROR;
                //    }
                //}
                //else
                //{
                //    StateObj = new MINIFoupTiltStateError(this);
                //    return EventCodeEnum.FOUP_ERROR;
                //}

            }
            catch (Exception err)
            {
                StateObj = new MINIFoupTiltStateError(this);
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. MINIFoupTilt - StateInit() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);
                return retVal;
            }
            return retVal;
        }
    }
    public abstract class MINIFoupTiltStateBase
    {
        public MINIFoupTiltStateBase(MINIFoupTilt owner)
        {
            this.Owner = owner;
        }

        private MINIFoupTilt _Owner;
        public MINIFoupTilt Owner
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

        public abstract TiltStateEnum GetState();

        public abstract EventCodeEnum Up();

        public abstract EventCodeEnum Down();

        public EventCodeEnum UpFunc()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if(Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    Owner.StateObj = new MINIFoupTiltStateUp(Owner);
                    retVal = EventCodeEnum.NONE;

                    return EventCodeEnum.NONE;
                }

                int retValue = -1;

                int devNum = Owner.FoupIOManager.IOServ.Outputs[Owner.FoupIOManager.IOMap.Outputs.DO_CSTT_AIR.ChannelIndex.Value].DevIndex;


                if (Owner.FoupIOManager.IOServ.IOList[devNum] is ECATIOProvider)
                {
                    try
                    {
                        retValue = FoupCylinderType.FoupCassetteTilting.Extend();

                        if (retValue == 0)
                        {
                            Owner.StateObj = new MINIFoupTiltStateUp(Owner);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            retVal = EventCodeEnum.FOUP_ERROR;
                            Owner.StateObj = new MINIFoupTiltStateError(Owner);
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                        Owner.StateObj = new MINIFoupTiltStateError(Owner);
                        LoggerManager.Debug($"{err.Message.ToString()}. FoupTiltUp - Up() : Error occured.");
                        LoggerManager.Exception(err);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);

                    }
                }
                else
                {
                    retVal = EventCodeEnum.FOUP_ERROR;
                    Owner.StateObj = new MINIFoupTiltStateError(Owner);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                Owner.StateTransition(new MINIFoupTiltStateError(Owner));
                retVal = EventCodeEnum.FOUP_ERROR;
            }

            return retVal;
        }

        public EventCodeEnum DownFunc()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            int devNum = Owner.FoupIOManager.IOServ.Outputs[Owner.FoupIOManager.IOMap.Outputs.DO_CSTT_AIR.ChannelIndex.Value].DevIndex;
            int retValue = -1;

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    Owner.StateObj = new MINIFoupTiltStateDown(Owner);
                    retVal = EventCodeEnum.NONE;

                    return EventCodeEnum.NONE;
                }

                if (Owner.FoupIOManager.IOServ.IOList[devNum] is ECATIOProvider)
                {
                    try
                    {
                        retValue = FoupCylinderType.FoupCassetteTilting.Retract();

                        if (retValue == 0)
                        {
                            Owner.StateObj = new MINIFoupTiltStateDown(Owner);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            retVal = EventCodeEnum.FOUP_ERROR;
                            Owner.StateObj = new MINIFoupTiltStateError(Owner);
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                        Owner.StateObj = new MINIFoupTiltStateError(Owner);
                        LoggerManager.Debug($"{err.Message.ToString()}. FoupTiltDown - Down() : Error occured.");
                        LoggerManager.Exception(err);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);

                    }
                }
                else
                {
                    retVal = EventCodeEnum.FOUP_ERROR;
                    Owner.StateObj = new MINIFoupTiltStateError(Owner);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                Owner.StateTransition(new MINIFoupTiltStateError(Owner));
                retVal = EventCodeEnum.FOUP_ERROR;
            }

            return retVal;
        }
    }

    public class MINIFoupTiltStateUp : MINIFoupTiltStateBase
    {
        public MINIFoupTiltStateUp(MINIFoupTilt owner) : base(owner)
        {
        }

        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.UP;
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
                Owner.StateTransition(new MINIFoupTiltStateError(Owner));
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
                Owner.StateTransition(new MINIFoupTiltStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class MINIFoupTiltStateDown : MINIFoupTiltStateBase
    {
        public MINIFoupTiltStateDown(MINIFoupTilt owner) : base(owner)
        {
        }

        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.DOWN;
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
                Owner.StateTransition(new MINIFoupTiltStateError(Owner));
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
                Owner.StateTransition(new MINIFoupTiltStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class MINIFoupTiltStateError : MINIFoupTiltStateBase
    {
        public MINIFoupTiltStateError(MINIFoupTilt owner) : base(owner)
        {
        }

        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.ERROR;
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
                Owner.StateTransition(new MINIFoupTiltStateError(Owner));
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

                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
    public class MINIFoupTiltNAState : MINIFoupTiltStateBase
    {
        public MINIFoupTiltNAState(MINIFoupTilt module) : base(module)
        {
        }
        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.IDLE;
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
                Owner.StateTransition(new MINIFoupTiltStateError(Owner));
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

                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
