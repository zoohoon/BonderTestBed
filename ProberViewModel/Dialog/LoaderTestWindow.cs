using System;
using System.Threading;

namespace LoaderTestDialog
{
    using LoaderTestMainPageView;
    using LoaderTestMainPageViewModel;
    using LogModule;
    using System.Windows;
    using System.Windows.Media;

    public static class LoaderTestWindow
    {
        private static Thread t;
        public static void AsyncShow()
        {
            try
            {
                if (t != null && t.IsAlive)
                    return;

                t = new Thread(STThread);
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private static void STThread()
        {
            try
            {
                VmLoaderTestMainPage vm = new VmLoaderTestMainPage();
                vm.InitModule();
                vm.InitViewModel();
                vm.PageSwitched(null);

                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    UcLoaderTestMainPage ucCognexOCRManualMainPage = new UcLoaderTestMainPage();
                    ucCognexOCRManualMainPage.DataContext = vm;

                    Window window = new Window()
                    {
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        //ResizeMode = ResizeMode.NoResize,
                        //WindowStyle = WindowStyle.None,
                        Background = Brushes.Black,
                        Height = 892,
                        Width = 1280,
                        Content = ucCognexOCRManualMainPage
                    };
                    //vm.SetWindow(window);

                    window.ShowDialog();

                    vm.DeInitViewModel();
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
