using System;
using System.Collections.Generic;
using ProberErrorCode;
using ProberInterfaces.Event.EventProcess;

namespace ProberInterfaces.Event
{
    public enum EventExecutorStateEnum
    {
        UNKNOWN = 0,
        IDLE,
        RUNNING,
        PAUSED,
        ERROR
    }

    /// <summary>
    /// Event를 생성/발생시켜주는 착한 친구.
    /// </summary>
    public interface IEventManager : IFactoryModule, IModule
    {
        List<IProbeEvent> EventList { get; set; }

        Queue<IProbeEvent> EventQueue { get; set; }
        List<string> EventHashCodeList { get; set; }
        List<ProbeEventInfo> EventFinishdedList { get; set; }
        List<string> RaisedEventList { get; set; }
        /// <summary>
        /// 이벤트 발생.
        /// </summary>
        /// <param name="evt">이벤트 클래스의 FullName</param>
        /// <param name="eventArg">Parameter</param>
        /// <returns></returns>
        EventCodeEnum RaisingEvent(string evt, ProbeEventArgs eventArg = null);
        /// <summary>
        /// 이벤트 비동기 실행 함수.
        /// </summary>
        /// <param name="eventfullname"></param>
        /// <param name="eventArg"></param>
        /// <returns></returns>
        //Task<EventCodeEnum> RaiseEvent(string eventfullname, ProbeEventArgs eventArg = null);
        /// <summary>
        /// 이벤트매니저에 이벤트 추가.
        /// </summary>
        /// <param name="eventfullname">Event Full Name</param>
        /// <param name="HandlerName"></param>
        /// <param name="EventHandle"></param>
        /// <returns></returns>
        EventCodeEnum RegisterEvent(string eventfullname, string HandlerName, EventHandler<ProbeEventArgs> EventHandle);

        /// <summary>
        /// 이벤트매니저에 이벤트를 리스트로 한번에 추가.
        /// </summary>
        /// <param name="subscribeRecipeParam"></param>
        /// <param name="handlerName"></param>
        /// <param name="moduleAlias"></param>
        /// <returns></returns>
        EventCodeEnum RegisterEventList(List<EventProcessBase> subscribeRecipeParam, string handlerName, string moduleAlias);


        /// <summary>
        /// 이벤트매니저에 이벤트 제거.
        /// </summary>
        /// <param name="eventfullname">Event Full Name</param>
        /// <param name="HandlerName"></param>
        /// <param name="EventHandle"></param>
        /// <returns></returns>
        EventCodeEnum RemoveEvent(string eventfullname, string HandlerName, EventHandler<ProbeEventArgs> EventHandle);

        /// <summary>
        /// 이벤트매니저에 이벤트를 리스트로 한번에 제거.
        /// </summary>
        /// <param name="subscribeRecipeParam"></param>
        /// <param name="handlerName"></param>
        /// <param name="moduleAlias"></param>
        /// <returns></returns>
        EventCodeEnum RemoveEventList(List<EventProcessBase> subscribeRecipeParam, string handlerName, string moduleAlias);
    }

    public interface IEventExecutor : IStateModule
    {

    }

    
}
