using System;
using ECATIO;
using ProberInterfaces.Foup;
using ProberErrorCode;
using CylType;
using LogModule;

namespace FoupModules.FoupTilt
{
    public class Foup8InchTilt : FoupTiltBase
    {
        public override TiltStateEnum State => StateObj.GetState();

        private Foup8InchTiltState _StageObj;
        public Foup8InchTiltState StateObj
        {
            get { return _StageObj; }
            set { _StageObj = value; }
        }
        public Foup8InchTilt(IFoupModule module) : base(module)
        {
            try
            {
                //State = new DockingPort12InchIdle(this);
                StateInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal;

            try
            {
                int ret1 = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CSTT_DOWN, true, FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME, FoupIOGlobalVar.IO_CHECK_TIME_OUT);


                if (ret1 == 0)
                {
                    StateObj = new FoupTiltUp(this);
                    retVal = EventCodeEnum.NONE;
                }
                else if (ret1 != 0)
                {
                    StateObj = new FoupTiltDown(this);
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    StateObj = new FoupTiltError(this);
                    return EventCodeEnum.FOUP_ERROR;
                }

            }
            catch (Exception err)
            {
                StateObj = new FoupTiltError(this);
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. Foup8InchTilt - StateInit() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);

                return retVal;
            }
            return retVal;
        }

        public override TiltStateEnum GetState()
        {
            return StateObj.GetState();
        }


        public override EventCodeEnum Up()
        {
            return StateObj.Up();
        }


        public override EventCodeEnum Down()
        {
            return StateObj.Down();
        }

    }
    public abstract class Foup8InchTiltState
    {
        public Foup8InchTiltState(Foup8InchTilt _module)
        {
            Module = _module;
        }
        private Foup8InchTilt _Module;

        public Foup8InchTilt Module
        {
            get { return _Module; }
            set { _Module = value; }
        }

        public abstract TiltStateEnum GetState();
        public abstract EventCodeEnum Up();
        public abstract EventCodeEnum Down();
    }
    public class FoupTiltUp : Foup8InchTiltState
    {
        public FoupTiltUp(Foup8InchTilt module) : base(module)
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
                int retValue = -1;

                int devNum = Module.FoupIOManager.IOServ.Outputs[Module.FoupIOManager.IOMap.Outputs.DO_CSTT_AIR.ChannelIndex.Value].DevIndex;


                if (Module.FoupIOManager.IOServ.IOList[devNum] is ECATIOProvider)
                {
                    retVal = EventCodeEnum.NONE;
                    try
                    {
                        retValue = FoupCylinderType.FoupCassetteTilting.Extend();

                        if (retValue == 0)
                        {
                            Module.StateObj = new FoupTiltUp(Module);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            throw new Exception($"Cassette_Tilt_Up()");
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                        Module.StateObj = new FoupTiltError(Module);
                        LoggerManager.Debug($"{err.Message.ToString()}. FoupTiltUp - Up() : Error occured.");
                        LoggerManager.Exception(err);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                    try
                    {
                        retValue = FoupCylinderType.FoupCassetteTilting.Extend();

                        if (retValue == 0)
                        {
                            Module.StateObj = new FoupTiltUp(Module);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            throw new Exception($"Cassette_Tilt_Up()");
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                        Module.StateObj = new FoupTiltError(Module);
                        LoggerManager.Debug($"{err.Message.ToString()}. FoupTiltUp - Up() : Error occured.");
                        LoggerManager.Exception(err);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum Down()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int retValue = -1;

                int devNum = Module.FoupIOManager.IOServ.Outputs[Module.FoupIOManager.IOMap.Outputs.DO_CSTT_AIR.ChannelIndex.Value].DevIndex;


                if (Module.FoupIOManager.IOServ.IOList[devNum] is ECATIOProvider)
                {
                    retVal = EventCodeEnum.NONE;
                    try
                    {
                        retValue = FoupCylinderType.FoupCassetteTilting.Retract();

                        if (retValue == 0)
                        {
                            Module.StateObj = new FoupTiltDown(Module);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            throw new Exception($"Cassette_Tilt_Down()");
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                        Module.StateObj = new FoupTiltError(Module);
                        LoggerManager.Debug($"{err.Message.ToString()}. FoupTiltUp - Down() : Error occured.");
                        LoggerManager.Exception(err);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);

                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                    try
                    {
                        retValue = FoupCylinderType.FoupCassetteTilting.Retract();

                        if (retValue == 0)
                        {
                            Module.StateObj = new FoupTiltDown(Module);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            throw new Exception($"Cassette_Tilt_Down()");
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                        Module.StateObj = new FoupTiltError(Module);
                        LoggerManager.Debug($"{err.Message.ToString()}. FoupTiltUp - Down() : Error occured.");
                        LoggerManager.Exception(err);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;

        }
    }
    public class FoupTiltDown : Foup8InchTiltState
    {
        public FoupTiltDown(Foup8InchTilt module) : base(module)
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
                int retValue = -1;

                int devNum = Module.FoupIOManager.IOServ.Outputs[Module.FoupIOManager.IOMap.Outputs.DO_CSTT_AIR.ChannelIndex.Value].DevIndex;


                if (Module.FoupIOManager.IOServ.IOList[devNum] is ECATIOProvider)
                {
                    retVal = EventCodeEnum.NONE;
                    try
                    {
                        retValue = FoupCylinderType.FoupCassetteTilting.Extend();

                        if (retValue == 0)
                        {
                            Module.StateObj = new FoupTiltUp(Module);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            throw new Exception($"Cassette_Tilt_Up()");
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                        Module.StateObj = new FoupTiltError(Module);
                        LoggerManager.Debug($"{err.Message.ToString()}. FoupTiltDown - Down() : Error occured.");
                        LoggerManager.Exception(err);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);

                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                    try
                    {
                        retValue = FoupCylinderType.FoupCassetteTilting.Extend();

                        if (retValue == 0)
                        {
                            Module.StateObj = new FoupTiltUp(Module);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            throw new Exception($"Cassette_Tilt_Up()");
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                        Module.StateObj = new FoupTiltError(Module);
                        LoggerManager.Debug($"{err.Message.ToString()}. FoupTiltDown - Up() : Error occured.");
                        LoggerManager.Exception(err);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum Down()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int retValue = -1;

                int devNum = Module.FoupIOManager.IOServ.Outputs[Module.FoupIOManager.IOMap.Outputs.DO_CSTT_AIR.ChannelIndex.Value].DevIndex;


                if (Module.FoupIOManager.IOServ.IOList[devNum] is ECATIOProvider)
                {
                    retVal = EventCodeEnum.NONE;
                    try
                    {
                        retValue = FoupCylinderType.FoupCassetteTilting.Retract();

                        if (retValue == 0)
                        {
                            Module.StateObj = new FoupTiltDown(Module);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            throw new Exception($"Cassette_Tilt_Down()");
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                        Module.StateObj = new FoupTiltError(Module);
                        LoggerManager.Debug($"{err.Message.ToString()}. FoupTiltDown - Down() : Error occured.");
                        LoggerManager.Exception(err);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);

                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                    try
                    {
                        retValue = FoupCylinderType.FoupCassetteTilting.Retract();

                        if (retValue == 0)
                        {
                            Module.StateObj = new FoupTiltDown(Module);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            throw new Exception($"Cassette_Tilt_Down()");
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                        Module.StateObj = new FoupTiltError(Module);
                        LoggerManager.Debug($"{err.Message.ToString()}. FoupTiltDown - Down() : Error occured.");
                        LoggerManager.Exception(err);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return EventCodeEnum.NONE;
        }
    }
    public class FoupTiltError : Foup8InchTiltState
    {
        public FoupTiltError(Foup8InchTilt module) : base(module)
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
                int retValue = -1;

                int devNum = Module.FoupIOManager.IOServ.Outputs[Module.FoupIOManager.IOMap.Outputs.DO_CSTT_AIR.ChannelIndex.Value].DevIndex;


                if (Module.FoupIOManager.IOServ.IOList[devNum] is ECATIOProvider)
                {
                    retVal = EventCodeEnum.NONE;
                    try
                    {
                        retValue = FoupCylinderType.FoupCassetteTilting.Extend();

                        if (retValue == 0)
                        {
                            Module.StateObj = new FoupTiltUp(Module);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            throw new Exception($"Cassette_Tilt_Up()");
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                        Module.StateObj = new FoupTiltError(Module);
                        LoggerManager.Debug($"{err.Message.ToString()}. FoupTiltUp - Up() : Error occured.");
                        LoggerManager.Exception(err);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                    try
                    {
                        retValue = FoupCylinderType.FoupCassetteTilting.Extend();

                        if (retValue == 0)
                        {
                            Module.StateObj = new FoupTiltUp(Module);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            throw new Exception($"Cassette_Tilt_Up()");
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                        Module.StateObj = new FoupTiltError(Module);
                        LoggerManager.Debug($"{err.Message.ToString()}. FoupTiltUp - Up() : Error occured.");
                        LoggerManager.Exception(err);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum Down()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int retValue = -1;

                int devNum = Module.FoupIOManager.IOServ.Outputs[Module.FoupIOManager.IOMap.Outputs.DO_CSTT_AIR.ChannelIndex.Value].DevIndex;


                if (Module.FoupIOManager.IOServ.IOList[devNum] is ECATIOProvider)
                {
                    retVal = EventCodeEnum.NONE;
                    try
                    {
                        retValue = FoupCylinderType.FoupCassetteTilting.Retract();

                        if (retValue == 0)
                        {
                            Module.StateObj = new FoupTiltDown(Module);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            throw new Exception($"Cassette_Tilt_Down()");
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                        Module.StateObj = new FoupTiltError(Module);
                        LoggerManager.Debug($"{err.Message.ToString()}. FoupTiltUp - Down() : Error occured.");
                        LoggerManager.Exception(err);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);

                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                    try
                    {
                        retValue = FoupCylinderType.FoupCassetteTilting.Retract();

                        if (retValue == 0)
                        {
                            Module.StateObj = new FoupTiltDown(Module);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            throw new Exception($"Cassette_Tilt_Down()");
                        }
                    }
                    catch (Exception err)
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                        Module.StateObj = new FoupTiltError(Module);
                        LoggerManager.Debug($"{err.Message.ToString()}. FoupTiltUp - Down() : Error occured.");
                        LoggerManager.Exception(err);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;

        }
    }

}
