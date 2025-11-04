using RequestInterface;

namespace ProberInterfaces.Event
{
    public interface ITCPIPEventParam
    {
        string CommandName { get; set; }

        RequestBase Response { get; set; }
    }

    public interface ITCPIPEvent : IProbeEvent
    {
    }
}

//    public interface ImyEvent
//    {
//        List<ImyEvent> evet { get; }

//        bool CanExecute();

//        void DoEvent();
//    }

//    class runeventclass
//    {

//    }

//    class runidleevstate
//    {
//        Eventbase v;

//        void Do()
//        {
//            v.CanExecute();

//            //state => send
//        }
//    }
//}
//public abstract class Eventbase : ImyEvent
//{


//    public void DoEvent()
//    {
//        PreAction();

//        //evet send;
//        for (int i = 0; i < evet; i++)
//        {

//        }

//        PostAction();
//    }

//    public abstract List<ImyEvent> evet { get; }

//    protected abstract void PreAction();
//    protected abstract void PostAction();
//}

//class zup : Eventbase
//{
//    public override List<ImyEvent> evet = new List<ImyEvent>();


//    protected override void PostAction()
//    {

//    }

//    protected override void PreAction()
//    {

//    }
//}

