using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProberInterfaces.Event;
using ProberInterfaces;
using System.ComponentModel;
using SequenceService;

namespace ProbeEvent
{
    using System.IO;
    using System.Reflection;
    using System.Collections.ObjectModel;
    using System.Windows;
    using ProberInterfaces.Command;
    using ProberErrorCode;
    using ProberInterfaces.Event.EventProcess;
    using ProberInterfaces.State;
    using ProberInterfaces.Wizard;
    using LogModule;
    using System.Runtime.CompilerServices;

    public class EventManager : IEventManager, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        private const string NotifyEventDLL = "NotifyEvent.dll";

        public bool Initialized { get; set; } = false;

        public Queue<IProbeEvent> EventQueue { get; set; }

        public List<string> EventHashCodeList { get; set; }

        public List<IProbeEvent> EventList { get; set; }

        public List<string> EventRecipeList { get; set; }
        public List<ProbeEventInfo> EventFinishdedList { get; set; }

        public List<string> RaisedEventList { get; set; }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    EventQueue = new Queue<IProbeEvent>();
                    EventRecipeList = new List<string>();
                    EventList = new List<IProbeEvent>();

                    EventFinishdedList = new List<ProbeEventInfo>();
                    EventHashCodeList = new List<string>();
                    RaisedEventList = new List<string>();

                    retval = LoadEventList();

                    Initialized = true;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum LoadEventList()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                //String strFolder = System.IO.Directory.GetCurrentDirectory();
                String strFolder = AppDomain.CurrentDomain.BaseDirectory;

                string LoadDLLPath = Path.Combine(strFolder, NotifyEventDLL);

                Assembly ass = Assembly.LoadFrom(LoadDLLPath);

                if (ass != null)
                {
                    foreach (var type in ass.GetTypes())
                    {
                        //var cmdInterfaceTypes = type.GetInterfaces().Where(x => typeof(NotifyEvent).IsAssignableFrom(x)).ToList();
                        if (type.IsAbstract == false)
                        {
                            var inst = Activator.CreateInstance(type) as IProbeEvent;
                            EventList.Add(inst);
                        }

                        //if (cmdInterfaceTypes.Count == 2)
                        //{
                        //    var foundType = cmdInterfaceTypes.Where(item => item != typeof(IProbeCommand)).First();
                        //    string cmdName = CommandNameGen.Generate(foundType);
                        //}
                        //else
                        //{
                        //    //err
                        //}

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

        private object lockObj = new object();
        public EventCodeEnum RaisingEvent(string eventfullname, ProbeEventArgs eventArg = null)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}], RaisingEvent() : Lock Start, eventFullName: {eventfullname}");

                lock (lockObj)
                {
                    IProbeEvent Evt = EventList.Find(x => x.GetType().FullName == eventfullname);

                    if (Evt != null)
                    {
                        RaisedEventList.Add(eventfullname);

                        if (eventArg != null)
                        {
                            Evt.EventArgQueue.Enqueue(eventArg);
                        }

                        LoggerManager.Debug($"[{this.GetType().Name}], RaisingEvent() : {Evt.GetType().Name}", isInfo: true);

                        EventQueue.Enqueue(Evt);

                        RetVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], RaisingEvent() : IProbeEvent is null, eventFullName: {eventfullname}");

                        RetVal = EventCodeEnum.UNDEFINED;
                    }
                }

                LoggerManager.Debug($"[{this.GetType().Name}], RaisingEvent() : Lock End, eventFullName: {eventfullname}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;

            //return EventList.Find(x => (x.GetType().BaseType.Name == EvtInfo.InterfaceName) &&
            //                           (x.GetType().Name == EvtInfo.ClassName));
        }


        public EventCodeEnum RegisterEventList(List<EventProcessBase> subscribeRecipeParam, string handlerName, string moduleAlias)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (subscribeRecipeParam != null)
                {
                    foreach (var evtname in subscribeRecipeParam)
                    {
                        RetVal = RegisterEvent(evtname.EventFullName, handlerName, evtname.EventNotify);

                        if (RetVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[{moduleAlias}] Regist EventSubscribe Error...");

                            break;
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        public EventCodeEnum RegisterEvent(string eventfullname, string HandlerName, EventHandler<ProbeEventArgs> EventHandle)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IProbeEvent Evt = EventList.Find(x => x?.GetType().FullName == eventfullname);

                if (Evt != null)
                {
                    if (Evt is INotifyEvent)
                    {
                        INotifyEvent e = Evt as INotifyEvent;

                        //e.ProbeEventSubscibers;

                        //Application.Current.Dispatcher.Invoke(() =>
                        //{
                        WeakEventManager<INotifyEvent, ProbeEventArgs>.AddHandler(e, HandlerName, EventHandle);
                        //});

                        RetVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        RetVal = EventCodeEnum.NODATA;
                    }
                }
                else
                {
                    RetVal = EventCodeEnum.NODATA;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum RemoveEventList(List<EventProcessBase> subscribeRecipeParam, string handlerName, string moduleAlias)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (var evtname in subscribeRecipeParam)
                {
                    RetVal = RemoveEvent(evtname.EventFullName, handlerName, evtname.EventNotify);

                    if (RetVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[{moduleAlias}] Regist EventSubscribe Error...");

                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        public EventCodeEnum RemoveEvent(string eventfullname, string HandlerName, EventHandler<ProbeEventArgs> EventHandle)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IProbeEvent Evt = EventList.Find(x => x.GetType().FullName == eventfullname);

                if (Evt != null)
                {
                    if (Evt is INotifyEvent)
                    {
                        INotifyEvent e = Evt as INotifyEvent;

                        //e.ProbeEventSubscibers;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            WeakEventManager<INotifyEvent, ProbeEventArgs>.RemoveHandler(e, HandlerName, EventHandle);
                        });

                        RetVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        RetVal = EventCodeEnum.NODATA;
                    }
                }
                else
                {
                    RetVal = EventCodeEnum.NODATA;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }
    }

    public class EventExecutor : SequenceServiceBase, IEventExecutor, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }
        public IEventManager EventManager { get; set; }

        private ModuleStateBase _ModuleState;

        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            private set { _ModuleState = value; }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.EVENT);
        public ReasonOfError ReasonOfError
        {
            get { return _ReasonOfError; }
            set
            {
                if (value != _ReasonOfError)
                {
                    _ReasonOfError = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<TransitionInfo> _TransitionInfo;
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set
            {
                if (value != _TransitionInfo)
                {
                    _TransitionInfo = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SubStepModules _SubModules;
        public ISubStepModules SubModules
        {
            get { return _SubModules; }
            set
            {
                if (value != _SubModules)
                {
                    _SubModules = (SubStepModules)value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    ModuleState = new ModuleInitState(this);
                    
                    _EventExecutorState = new EventExecutorStateIdleState(this);

                    _TransitionInfo = new ObservableCollection<TransitionInfo>();

                    EventManager = this.EventManager();

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

        }
        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventExecutorState _EventExecutorState;

        public EventExecutorState EventExecutorState
        {
            get { return _EventExecutorState; }
        }

        public IInnerState InnerState
        {
            get { return _EventExecutorState; }
            set
            {
                if (value != _EventExecutorState)
                {
                    _EventExecutorState = value as EventExecutorState;
                }
            }
        }

        public IInnerState PreInnerState { get; set; }

        private CommandSlot _CommandRecvSlot = new CommandSlot();
        public CommandSlot CommandRecvSlot
        {
            get { return _CommandRecvSlot; }
            set { _CommandRecvSlot = value; }
        }

        private CommandSlot _CommandProcSlot = new CommandSlot();
        public CommandSlot CommandRecvProcSlot
        {
            get { return _CommandProcSlot; }
            set { _CommandProcSlot = value; }
        }

        private CommandSlot _CommandRecvDoneSlot = new CommandSlot();
        public CommandSlot CommandRecvDoneSlot
        {
            get { return _CommandRecvDoneSlot; }
            set { _CommandRecvDoneSlot = value; }
        }

        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }
        private CommandTokenSet _RunTokenSet;

        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }

        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                PreInnerState = InnerState;
                InnerState = state;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsBusy()
        {
            bool retVal = false;
            try
            {
                foreach (var subModule in SubModules.SubModules)
                {
                    if (subModule.GetMovingState() == MovingStateEnum.MOVING)
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override ModuleStateEnum SequenceRun()
        {
            ModuleStateEnum RetVal = ModuleStateEnum.UNDEFINED;

            try
            {
                Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public EventCodeEnum ClearState()  //Data 초기화 함=> Done에서 IDLE 상태로 넘어감
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = InnerState.ClearState();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        {
            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            try
            {
                InnerState.End();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        public ModuleStateEnum Abort()
        {
            try
            {
                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public ModuleStateEnum Execute() // Don`t Touch
        {
            ModuleStateEnum stat = ModuleStateEnum.ERROR;
            try
            {
                EventCodeEnum retVal = InnerState.Execute();
                ModuleState.StateTransition(InnerState.GetModuleState());
                RunTokenSet.Update();
                stat = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return stat;
        }

        //public void RunAsync()
        //{
        //    throw new NotImplementedException();
        //}

        public void StateTransition(ModuleStateBase state)
        {
            ModuleState = state;
        }



        public bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            msg = "";
            return true;
        }

        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }

    }
}
