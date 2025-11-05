//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace EventProcessModule.TEST
//{
//    using Autofac;
//    using LogModule;
//    using NotifyEventModule;
//    using ProberInterfaces;
//    using ProberInterfaces.Command;
//    using ProberInterfaces.Command.Cui;
//    using ProberInterfaces.Event;
//    using ProberInterfaces.Event.EventProcess;
//    using ProberInterfaces.LogInterpreter;

//    public abstract class NickTestProcessBase : EventProcessBase
//    {
//    }

//    //==> Lot Start
//    public class NickTestEventProcess_Event1 : NickTestProcessBase
//    {
//        public override void EventNotify(object sender, ProbeEventArgs e)
//        {
//            NormalMarkerWrapper actionSource = e.Sender as NormalMarkerWrapper;

//            LoggerManager.Debug($"[{actionSource.SenderName}][{this.GetType().FullName}][{LoggerManager.GetCallerName()}] Start...");
//            LoggerManager.Debug($"[{actionSource.SenderName}][{this.GetType().FullName}][{LoggerManager.GetCallerName()}] End...");
//        }
//    }

//    //==> Lot Pause
//    public class NickTestEventProcess_Event2 : NickTestProcessBase
//    {
//        public override void EventNotify(object sender, ProbeEventArgs e)
//        {
//            NormalMarkerWrapper actionSource = e.Sender as NormalMarkerWrapper;

//            LoggerManager.Debug($"[{actionSource.SenderName}][{this.GetType().FullName}][{LoggerManager.GetCallerName()}] Start...");
//            LoggerManager.Debug($"[{actionSource.SenderName}][{this.GetType().FullName}][{LoggerManager.GetCallerName()}] End...");
//        }
//    }

//    //==> Lot End
//    public class NickTestEventProcess_Event3 : NickTestProcessBase
//    {
//        public override void EventNotify(object sender, ProbeEventArgs e)
//        {
//            NormalMarkerWrapper actionSource = e.Sender as NormalMarkerWrapper;

//            LoggerManager.Info($"[{actionSource.SenderName}][{this.GetType().FullName}][{LoggerManager.GetCallerName()}] Start...");
//            this.CommandManager().SetCommand(
//                actionSource,
//                CommandNameGen.Generate(typeof(ITestCommand3)),
//                new TestCommand3Param());

//            LoggerManager.Info($"[{actionSource.SenderName}][{this.GetType().FullName}][{LoggerManager.GetCallerName()}] End...");
//        }
//    }
//}
