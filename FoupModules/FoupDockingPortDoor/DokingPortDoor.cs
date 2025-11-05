
using System;
using ProberInterfaces;
using ProberInterfaces.Foup;
using ProberErrorCode;
using LogModule;

namespace FoupModules.DockingPortDoor
{
    public class DockingPortDoorNomal : DockingPortDoorBase
    {
        private DPDoorState _State;

        public DPDoorState State
        {
            get { return _State; }
            set { _State = value; }
        }

        public DockingPortDoorNomal(IFoupModule module) : base(module)
        {
            StateInit();
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
                        case DockingPortDoorStateEnum.OPEN:
                            State = new DockingPortDoorOpen(this);
                            break;
                        case DockingPortDoorStateEnum.CLOSE:
                            State = new DockingPortDoorClose(this);
                            break;
                        case DockingPortDoorStateEnum.IDLE:
                            State = new DockingPortDoorError(this);
                            break;
                        case DockingPortDoorStateEnum.ERROR:
                            State = new DockingPortDoorError(this);
                            break;
                        default:
                            break;
                    }

                    return EventCodeEnum.NONE;
                }

                bool value;

                int ret = FoupIOManager.ReadBit(FoupIOManager.IOMap.Inputs.DI_FO_OPEN, out value);

                if (ret != 0)
                {
                    State = new DockingPortDoorError(this);
                    return EventCodeEnum.FOUP_ERROR;
                }
                else
                {
                    if (value == true)
                    {
                        State = new DockingPortDoorOpen(this);
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        State = new DockingPortDoorClose(this);
                        retVal = EventCodeEnum.NONE;
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
        public override DockingPortDoorStateEnum GetState()
        {
            return State.GetState();
        }

        public override EventCodeEnum Open()
        {
            return State.Open();
        }

        public override EventCodeEnum Close()
        {
            return State.Close();
        }
    }
    public abstract class DPDoorState
    {
        public DPDoorState(DockingPortDoorNomal _module)
        {
            Module = _module;
        }
        private DockingPortDoorNomal _Module;

        public DockingPortDoorNomal Module
        {
            get { return _Module; }
            set { _Module = value; }
        }

        public abstract DockingPortDoorStateEnum GetState();

        public abstract EventCodeEnum Open();
        public abstract EventCodeEnum Close();
    }

    public class DockingPortDoorOpen : DPDoorState
    {
        public DockingPortDoorOpen(DockingPortDoorNomal module) : base(module)
        {
        }
        public override DockingPortDoorStateEnum GetState()
        {
            return DockingPortDoorStateEnum.OPEN;
        }

        public override EventCodeEnum Open()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum Close()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.State = new DockingPortDoorClose(Module);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }


    }
    public class DockingPortDoorClose : DPDoorState
    {
        public DockingPortDoorClose(DockingPortDoorNomal module) : base(module)
        {
        }
        public override DockingPortDoorStateEnum GetState()
        {
            return DockingPortDoorStateEnum.CLOSE;
        }

        public override EventCodeEnum Open()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.State = new DockingPortDoorOpen(Module);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum Close()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }


    }

    //public class DockingPortDoorIdle : DPDoorState
    //{
    //    public DockingPortDoorIdle(DockingPortDoorNomal module) : base(module)
    //    {
    //    }
    //    public override DockingPortDoorState GetState()
    //    {
    //        return DockingPortDoorState.Idle;
    //    }

    //    public override EventCodeEnum Open()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override EventCodeEnum Close()
    //    {
    //        throw new NotImplementedException();
    //    }


    //}

    public class DockingPortDoorError : DPDoorState
    {
        public DockingPortDoorError(DockingPortDoorNomal module) : base(module)
        {
        }
        public override DockingPortDoorStateEnum GetState()
        {
            return DockingPortDoorStateEnum.ERROR;
        }

        public override EventCodeEnum Open()
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum Close()
        {
            throw new NotImplementedException();
        }


    }
}
