using LogModule;
using System;
using System.Windows;

namespace SplasherService
{
    public static class Splasher
    {
        public static Window Splash { get; set; }

        public static object GetDataContext()
        {
            return Splash.DataContext;
        }

        public static void ShowSplash()
        {
            Splash?.Show();
        }

        /// <summary>
        /// Close splash screen
        /// </summary>
        public static void CloseSplash()
        {
            try
            {
                Splash.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (Splash != null)
                        {
                            Splash.Close();
                            System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
                            if (Splash is IDisposable)
                                (Splash as IDisposable).Dispose();
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }

                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
