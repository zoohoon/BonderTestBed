using PreCheckerManager;
using System;

namespace PreChecker
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ExcuteManager excuteManager = new ExcuteManager();
            excuteManager.Start(args);
        }
    }
}
