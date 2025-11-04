//using LogModule;
//using ProberErrorCode;
//using ProberInterfaces;
//using ProberInterfaces.Communication;
//using ProberInterfaces.Enum;
//using ProberInterfaces.EnvControl.Enum;
//using ProberInterfaces.State;
//using ProberInterfaces.Temperature.EnvMonitoring;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace EnvMonitoring
//{
//    public abstract class EnvMonitoringState : IInnerState
//    {
//        private ISensorModule _Module;
//        public ISensorModule Module
//        {
//            get { return _Module; }
//            private set { _Module = value; }
//        }
//        public EnvMonitoringState(ISensorModule module)
//        {
//            Module = module;
//        }

//        public abstract EventCodeEnum Execute();
//        public abstract EventCodeEnum Pause();
//        public abstract SensorStatusEnum GetState();
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
//    // sensor disconnect state


//    /// <summary>
//    /// Initial 하기 위한 Status, Disconnect State
//    /// Initial 끝났는데 SensorStatus가 Normal이면 NormalStatus로 전환되고 
//    /// Initial 끝났는데 SensorStatus가 Error이면 ErrorStatus로 전환된다.
//    /// </summary>
//    public class EnvMonitoringIdleState : EnvMonitoringState
//    {
//        public EnvMonitoringIdleState(ISensorModule sensorModule) : base(sensorModule)
//        {
//            Module.SensorStatus = SensorStatusEnum.INIT;
//        }
//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
//            try
//            {
//                IEnvMonitoringHub envMonitoringHub = Module.EnvMonitoringManager().EnvMonitoringHubs.Find(a => a.CommunicationParam.Hub.Value == Module.SensorSysParameter.Hub.Value);
//                if (envMonitoringHub != null)
//                {
//                    if (envMonitoringHub.CommModule.CurState == EnumCommunicationState.CONNECTED)
//                    {
//                        Module.InnerStateTransition(new EnvMonitoringNormalState(Module));
//                        ret = EventCodeEnum.NONE;
//                    }
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
//            return ModuleStateEnum.INIT;
//        }

//        public override SensorStatusEnum GetState()
//        {
//            return SensorStatusEnum.INIT;
//        }

//        public override EventCodeEnum Pause()
//        {
//            return EventCodeEnum.NONE;
//        }
//    }

//    /// <summary>
//    /// 센서가 정상적으로 동작하고 있다.
//    /// </summary>
//    public class EnvMonitoringNormalState : EnvMonitoringState
//    {
//        public EnvMonitoringNormalState(ISensorModule sensorModule) : base(sensorModule)
//        {
//            Module.SensorStatus = SensorStatusEnum.NORMAL;
//        }
//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
//            try
//            {
//                //GemSVID Update
//                //1번만 동작
//                if (Module.ErrorReasons.Count > 0 || Module.SensorStatus != SensorStatusEnum.NORMAL)
//                {                    
//                    Module.InnerStateTransition(new EnvMonitoringErrorState(Module));
//                }
//                //else
//                //{
//                //    // sensor module 은 idle, running, error state
//                //    // hub module 은 commodule 의 connect, disconnect state
                    
//                //    // 내가(센서) 어느 허브에 연결되어있는지 확인하는 작업
//                //    IEnvMonitoringHub envMonitoringHub = Module.EnvMonitoringManager().EnvMonitoringHubs.Find(a => a.CommunicationParam.Hub.Value == Module.SensorSysParameter.Hub.Value);
//                //    if(envMonitoringHub != null)
//                //    {
//                //        Module.bRcvDone = false;

//                //        if (!Module.SensorEnable)
//                //            Module.SensorEnable = true;

//                //        ret = Module.RequestData(Module.SensorIndex, envMonitoringHub);

//                //        Stopwatch ts = new Stopwatch(); //timer
//                //        ts.Start();

//                //        while (Module.bRcvDone == false)
//                //        {
//                //            if (ts.Elapsed.Seconds > 1)
//                //            {
//                //                // 1sec. Time-out"                                
//                //                LoggerManager.Debug($"The request for data has timed out.");
//                //            }
//                //        }
//                //    }
//                //}
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
//            return ModuleStateEnum.RUNNING;
//        }

//        public override SensorStatusEnum GetState()
//        {
//            return SensorStatusEnum.NORMAL;
//        }

//        public override EventCodeEnum Pause()
//        {
//            return EventCodeEnum.NONE;
//        }
//    }
//    /// <summary>
//    /// 센서에 어떠한 알람이 발생하였거나, 문제(disconnect)가 발생하였을때,
//    /// </summary>
//    // Error 가 발생했을때, after action 을 하기 위해 만든 class
//    public class EnvMonitoringErrorState : EnvMonitoringState
//    {
//        public EnvMonitoringErrorState(ISensorModule sensorModule) : base(sensorModule)
//        {            
//            Module.SensorStatus = SensorStatusEnum.ERROR;
//        }
//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

//            if(Module.SensorStatus == SensorStatusEnum.NORMAL)
//            {
//                //Transition Normal 
//            }


//            //GemSVID Update    // 여기서 하는게 아니라 sensor  module 에서 하는게 맞는거 아닌가 ? 상태나 온도가 변했을때만?
//            //1번만 동작

//            //if (Module.SensorStatus == SensorStatusEnum.DISCONNECT_HUB
//            //    || Module.SensorStatus == SensorStatusEnum.DISCONNECT_INDICATOR)
//            //{
//            //    Module.GEMSensorStatus = GEMSensorStatusEnum.DISCONNECTED;
//            //}
//            //else if (Module.SensorStatus == SensorStatusEnum.TEMP_ALARM
//            //    || Module.SensorStatus == SensorStatusEnum.TEMP_WARN
//            //    || Module.SensorStatus == SensorStatusEnum.SMOKE_DETECTED)
//            //{
//            //    Module.GEMSensorStatus = GEMSensorStatusEnum.ALARM;
//            //}

//            ////Notify
//            //if (Module.SensorStatus == SensorStatusEnum.TEMP_ALARM)
//            //{
//            //    if (!Module.bAlarmNotifyDone)
//            //    {
//            //        Module.bAlarmNotifyDone = true;
//            //        Module.NotifyManager().Notify(EventCodeEnum.SENSOR_ALARM_DETECTED);
//            //        // 여기서 NotifyManager 호출하는게 맞는가 ?
//            //    }
//            //}

//            // normal -> disconnect 되었을때, 


//            //AfterAction


//            //try
//            //{       
//            //    // 에러 해결이 되었는지 알려면, send 를 해봐야한다.
//            //    IEnvMonitoringHub envMonitoringHub = Module.EnvMonitoringManager().EnvMonitoringHubs.Find(a => a.CommunicationParam.Hub.Value == Module.SensorSysParameter.Hub.Value);
//            //    if(envMonitoringHub != null)
//            //    {
//            //        if (envMonitoringHub.CommModule.CurState == EnumCommunicationState.CONNECTED)
//            //        {
//            //            Module.bRcvDone = false;
//            //            ret = Module.RequestData(Module.SensorIndex, envMonitoringHub);

//            //            Stopwatch ts = new Stopwatch(); //timer
//            //            ts.Start();

//            //            while (Module.bRcvDone == false)
//            //            {                            
//            //                if (ts.Elapsed.Seconds > 1)
//            //                {
//            //                    // 1sec. Time-out"                                
//            //                    LoggerManager.Debug($"The request for data has timed out.");
//            //                }
//            //            }                                                                        

//            //            if (Module.ErrorReasons.Count == 0)
//            //            {
//            //                Module.InnerStateTransition(new EnvMonitoringIdleState(Module));
//            //            }                        
//            //        }
//            //        ret = EventCodeEnum.NONE;
//            //    }                
//            //}
//            //catch (Exception err)
//            //{
//            //    LoggerManager.Exception(err);
//            //    throw;
//            //}
//            return ret;
//        }

//        public override ModuleStateEnum GetModuleState()
//        {
//            return ModuleStateEnum.ERROR;
//        }

//        public override SensorStatusEnum GetState()
//        {
//            return SensorStatusEnum.ERROR;
//        }

//        public override EventCodeEnum Pause()
//        {
//            return EventCodeEnum.NONE;
//        }
//    }
//}
