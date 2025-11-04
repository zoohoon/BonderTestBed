namespace ProberInterfaces
{
    public delegate bool CheckRunMethod();

    public interface ISequenceEngineService
    {
        //List<CheckRunMethod> SequenceRunStates{ get; set; }
        //bool CanRunningState();
        ModuleStateEnum SequenceRun();
        void RunSequencer();
        void StopSequencer();
        void PauseSequencer(object caller);
        void ResumeSequencer();
        void FlushQueue();
        //int SequenceEnqueue(ISequence seq);
        //int Add(ISequence seq);
        //int Remove(ISequence seq);
        int WaitForSequencer(long timeout = 0);
        EnumEngineStatus Status { get; }
        string ThreadName { get; set; }

        //IModule Service { get; set; }
    }

    //public interface ISequenceEngine
    //{
    //    void RunSequencer();
    //    void StopSequencer();
    //    void PauseSequencer(object caller);
    //    void ResumeSequencer();
    //    void FlushQueue();
    //    //int SequenceEnqueue(ISequence seq);
    //    //int Add(ISequence seq);
    //    //int Remove(ISequence seq);
    //    int WaitForSequencer(long timeout = 0);
    //    EnumEngineStatus RunStatus { get; }
    //    IModule Job { set; }
    //    IModule EventServiceJob { set; }
    //}
    //public interface ISequence
    //{
    //    //DateTime InjectionTime { get; }
    //    //Queue<object> Injector { get; }
    //    //List<object> Requestors { get; }
    //    //void SetInjector(object obj);
    //    EnumSeqStatus SeqStatus();
    //    EnumSeqStatus Run();
    //    EnumSeqStatus FreeRun();
    //    EnumSeqStatus Pause();
    //    EnumSeqStatus Resume();
    //    EnumSeqStatus Abort();
    //    void RunAsync();
    //}

    //public abstract class SequenceBase : ISequence
    //{

    //    //private Queue<object> _Injector;

    //    //public Queue<object> Injector
    //    //{
    //    //    get { return _Injector; }
    //    //}
    //    //private List<object> _Requestors;

    //    //public List<object> Requestors
    //    //{
    //    //    get { return _Requestors; }
    //    //    set { _Requestors = value; }
    //    //}


    //    //private DateTime _InjectionTime;

    //    //public DateTime InjectionTime
    //    //{
    //    //    get { return _InjectionTime; }
    //    //    set { _InjectionTime = value; }
    //    //}

    //    public abstract EnumSeqStatus Abort();
    //    public abstract EnumSeqStatus Pause();
    //    public abstract EnumSeqStatus Resume();
    //    public abstract EnumSeqStatus Run();
    //    public abstract void RunAsync();
    //    public abstract EnumSeqStatus SeqStatus();
    //    //public void SetInjector(object obj)
    //    //{
    //    //    _Injector.Enqueue(obj);
    //    //    _InjectionTime = DateTime.Now;
    //    //}

    //    public abstract EnumSeqStatus FreeRun();
    //}

    public enum EnumSeqStatus
    {
        IDLE = 0,
        RUN = 1,
        DONE = 2,
        SUSPENDED = 3,
        PAUSED = 4,
        ABORTED = 5,
        ERROR = -1
    }

    public enum EnumEngineStatus
    {
        UNDEFINED = -1,
        IDLE = 0,
        RUNNING = 1,
        PAUSED = 2,
        ERROR = -1
    }
}
