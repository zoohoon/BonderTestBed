using System;
using System.Linq;
using System.Threading.Tasks;
using ProberInterfaces;
using ProberInterfaces.Event;
using ProberErrorCode;
using LogModule;
using ProberInterfaces.State;
//using ProberInterfaces.ThreadSync;

namespace ProbeEvent
{
    public abstract class EventExecutorState : IInnerState
    {
        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();
        public abstract EventExecutorStateEnum GetState();
        public abstract ModuleStateEnum GetModuleState();

        public virtual EventCodeEnum End()
        {
            throw new NotImplementedException();
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }

        public abstract EventCodeEnum ClearState();

        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }
    }

    public abstract class EventExecutorStateBase : EventExecutorState
    {
        private EventExecutor _Module;

        public EventExecutor Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        private static object EventListlock = new object();

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new EventExecutorStateIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void LockEventList_Add(string code)
        {
            try
            {
                lock (EventListlock)
                {
                    Module.EventManager.EventHashCodeList.Add(code);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LockEventList_Remove(string code)
        {
            try
            {
                lock (EventListlock)
                {
                    Module.EventManager.EventHashCodeList.Remove(code);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LockEventFinishedList_Add(ProbeEventInfo Evt)
        {
            try
            {
                lock (EventListlock)
                {
                    Module.EventManager.EventFinishdedList.Add(Evt);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LockEventFinishedList_Remove(ProbeEventInfo Evt)
        {
            try
            {
                lock (EventListlock)
                {
                    Module.EventManager.EventFinishdedList.Remove(Evt);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventExecutorStateBase(EventExecutor module)
        {
            try
            {
                Module = module;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ProbeEventInfo MakeProbeInfo(IProbeEvent CurEvent)
        {
            ProbeEventInfo EventInfo = new ProbeEventInfo();

            try
            {
                EventInfo.Event = CurEvent;

                EventInfo.StartTime = DateTime.Now;

                EventInfo.HashCode = SecuritySystem.SecurityUtil.GetHashCode_SHA256((EventInfo.StartTime.Ticks + EventInfo.Event.GetType().FullName));

                EventInfo.description = "TEST";

                EventInfo.eventtype = ProbeEventType.UNKNOWN;

                EventInfo.EventArgQueue = CurEvent.EventArgQueue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventInfo;
        }

        public async void DoEventAsynchronous(ProbeEventInfo evt)
        {
            try
            {
                EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

                ProbeEventArgs args = null;

                if (0 < evt.EventArgQueue.Count)
                {
                    args = evt.EventArgQueue.Dequeue();
                }

                RetVal = await Task.Run(
                    () =>
                    {
                        RetVal = EventCodeEnum.UNDEFINED;

                        try
                        {
                            RetVal = evt.Event.DoEvent(args);
                            Module.EventManager.RaisedEventList.Remove(evt.Event.ToString());
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }

                        return RetVal;
                    });

                args?.SamaphoreRelease();

                evt.EndTime = DateTime.Now;
                evt.Status = EventCodeEnum.EVENT_END;

                LockEventList_Remove(evt.HashCode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class EventExecutorStateIdleState : EventExecutorStateBase
    {
        public EventExecutorStateIdleState(EventExecutor module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                Module.InnerStateTransition(new EventExecutorStateRunningState(Module));
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public override EventExecutorStateEnum GetState()
        {
            return EventExecutorStateEnum.IDLE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class EventExecutorStateRunningState : EventExecutorStateBase
    {
        public EventExecutorStateRunningState(EventExecutor module) : base(module)
        {
        }
        private object lockobj = new object();

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                lock (lockobj)
                {
                    int EventCount = Module.EventManager.EventQueue.Count();

                    if (EventCount > 0)
                    {
                        ProbeEventInfo EventInfo = MakeProbeInfo(Module.EventManager.EventQueue.Dequeue());

                        // Do Event
                        DoEventAsynchronous(EventInfo);

                        LockEventList_Add(EventInfo.HashCode);
                    }
                }

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public override EventExecutorStateEnum GetState()
        {
            return EventExecutorStateEnum.RUNNING;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
}
