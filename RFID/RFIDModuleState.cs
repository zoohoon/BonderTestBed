//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ProberInterfaces.Error;
//using ProberInterfaces.RFID;
//using ProberInterfaces;
//using ProberErrorCode;
//using ProberInterfaces.State;
//using LogModule;

//namespace RFID
//{
//    public abstract class RFIDModuleStateBase : IInnerState
//    {
//        protected RFIDModule _Module;

//        public RFIDModuleStateBase()
//        {
//        }
//        public RFIDModuleStateBase(RFIDModule module)
//        {
//            _Module = module;
//        }

//        public abstract EventCodeEnum Execute();
//        public abstract EventCodeEnum Pause();
//        public abstract RFIDStateEnum GetState();
//        public abstract ModuleStateEnum GetModuleState();

//        public virtual EventCodeEnum End()
//        {
//            return EventCodeEnum.NONE;
//        }
//        public virtual EventCodeEnum Abort()
//        {
//            return EventCodeEnum.NONE;
//        }
//        public virtual EventCodeEnum ClearState()
//        {
//            return EventCodeEnum.NONE;
//        }
//        public virtual EventCodeEnum Resume()
//        {
//            return EventCodeEnum.NONE;
//        }
//    }
//    public class RFIDIdleState : RFIDModuleStateBase
//    {
//        public RFIDIdleState()
//        {

//        }
//        public RFIDIdleState(RFIDModule Rfidmodules) : base(Rfidmodules)
//        {

//        }

//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
//            try
//            {
//                //foup에서 해당되는 무언가가 true가 됫을때

//                ret = EventCodeEnum.NONE;
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw;
//            }
//            return ret;
//        }

//        public override ModuleStateEnum GetModuleState()
//        {
//            return ModuleStateEnum.IDLE;
//        }

//        public override RFIDStateEnum GetState()
//        {
//            return RFIDStateEnum.IDLE;
//        }

//        public override EventCodeEnum Pause()
//        {
//            return EventCodeEnum.NONE;
//        }
//    }
//    public class RFIDRunState : RFIDModuleStateBase
//    {
//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
//            try
//            {
//                if (_Module.gclsRFID_Online == true)
//                {
//                    bool retVal = _Module.RFID_RD_DATA();
//                    if (retVal == true)
//                    {

//                        ret = EventCodeEnum.NONE;
//                    }
//                    else
//                    {

//                        ret = EventCodeEnum.RFID_ERROR;
//                    }

//                }
//                else
//                {

//                }
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw;
//            }
//            return ret;
//        }

//        public override ModuleStateEnum GetModuleState()
//        {
//            return ModuleStateEnum.RUNNING;
//        }

//        public override RFIDStateEnum GetState()
//        {
//            return RFIDStateEnum.RUN;
//        }

//        public override EventCodeEnum Pause()
//        {
//            return EventCodeEnum.NONE;
//        }
//    }
//    public class RFIDDoneState : RFIDModuleStateBase
//    {
//        public override EventCodeEnum Execute()
//        {
//            throw new NotImplementedException();
//        }

//        public override ModuleStateEnum GetModuleState()
//        {
//            return ModuleStateEnum.DONE;
//        }

//        public override RFIDStateEnum GetState()
//        {
//            return RFIDStateEnum.DONE;
//        }

//        public override EventCodeEnum Pause()
//        {
//            return EventCodeEnum.NONE;
//        }
//    }
//    public class RFIDPauseState : RFIDModuleStateBase
//    {
//        public override EventCodeEnum Execute()
//        {
//            throw new NotImplementedException();
//        }

//        public override ModuleStateEnum GetModuleState()
//        {
//            return ModuleStateEnum.PAUSED;
//        }

//        public override RFIDStateEnum GetState()
//        {
//            return RFIDStateEnum.PAUSED;
//        }

//        public override EventCodeEnum Pause()
//        {
//            return EventCodeEnum.NONE;
//        }
//    }
//    public class RFIDErrorState : RFIDModuleStateBase
//    {
//        public override EventCodeEnum Execute()
//        {
//            throw new NotImplementedException();
//        }

//        public override ModuleStateEnum GetModuleState()
//        {
//            return ModuleStateEnum.ERROR;
//        }

//        public override RFIDStateEnum GetState()
//        {
//            return RFIDStateEnum.ERROR;
//        }

//        public override EventCodeEnum Pause()
//        {
//            return EventCodeEnum.NONE;
//        }
//    }

//}
