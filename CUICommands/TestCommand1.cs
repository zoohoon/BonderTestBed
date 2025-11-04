//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CUICommands
//{
//    using Autofac;
//    using LogModule;
//    using NotifyEventModule;
//    using ProberInterfaces;
//    using ProberInterfaces.Command;
//    using ProberInterfaces.Command.Cui;
//    using ProberInterfaces.Event;
//    using ProberInterfaces.LogInterpreter;
//    using System.Threading;

//    public class TestCommand1 : ProbeCommand, ITestCommand1
//    {
//        public override bool Execute()
//        {
//            TestCommand1Param param = Parameter as TestCommand1Param;
//            if (param == null)
//                return false;

//            NormalMarkerWrapper actionSource = Marker;
//            LoggerManager.Info($"[{actionSource.SenderName}][{this.GetType().FullName}][{LoggerManager.GetCallerName()}] Start...");
//            LoggerManager.Info($"[{actionSource.SenderName}][{this.GetType().FullName}][{LoggerManager.GetCallerName()}] End...");

//            return true;
//        }
//    }
//    public class TestCommand2 : ProbeCommand, ITestCommand2, IFactoryModule
//    {
//        public override bool Execute()
//        {
//            TestCommand2Param param = Parameter as TestCommand2Param;
//            if (param == null)
//                return false;

//            NormalMarkerWrapper actionSource = Marker;
//            LoggerManager.Info($"[{actionSource.SenderName}][{this.GetType().FullName}][{LoggerManager.GetCallerName()}] Start...");

//            //==> EX1) Source를 판별 할 때, 
//            this.EventManager().RaisingEvent(
//                typeof(LotStartEvent).FullName,
//                new ProbeEventArgs(actionSource) );

//            OtherFunction();

//            LoggerManager.Info($"[{actionSource.SenderName}][{this.GetType().FullName}][{LoggerManager.GetCallerName()}] End...");
//            return true;
//        }
//        private void OtherFunction()
//        {
//            //==> EX2) Source를 판별 못할 때
//            this.EventManager().RaisingEvent(
//                typeof(LotPausedEvent).FullName,
//                new ProbeEventArgs() { Sender = this });
//        }
//    }
//    public class TestCommand3 : ProbeCommand, ITestCommand3
//    {
//        public override bool Execute()
//        {
//            TestCommand3Param param = Parameter as TestCommand3Param;
//            if (param == null)
//                return false;

//            NormalMarkerWrapper actionSource = Marker;

//            LoggerManager.Info($"[{actionSource.SenderName}][{this.GetType().FullName}][{LoggerManager.GetCallerName()}] Start...");

//            //Task.Run(() =>
//            //{
//            //    Thread.Sleep(2000);
//            //    LoggerManager.ProLoggerCtl.Info($"[{this.GetType().FullName}][{LoggerManager.GetCallerName()}] Some Job finish...");
//            //});

//            LoggerManager.Info($"[{actionSource.SenderName}][{this.GetType().FullName}][{LoggerManager.GetCallerName()}] End...");

//            return true;
//        }
//    }
//}
