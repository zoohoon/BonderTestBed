namespace ProberInterfaces.Event
{
    public interface IGpibEventParam
    {
        string CommandName { get; set; }
        int StbNumber { get; set; }
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

