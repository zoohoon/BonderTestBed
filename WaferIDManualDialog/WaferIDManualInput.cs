using ProberInterfaces;
using System;
using System.Windows;
using System.Windows.Media;

namespace WaferIDManualDialog
{
    public static class WaferIDManualInput
    {
        private static VmWaferIDMainPage _VM;

        public static void Show(Autofac.IContainer LoaderContainer, TransferObject transfer = null)
        {

            STAThread(LoaderContainer, transfer);
        }

        private static void STAThread(Autofac.IContainer LoaderContainer, TransferObject transfer = null)
        {
            _VM = new VmWaferIDMainPage();
            _VM._Container = LoaderContainer;
            _VM.InitModule(transfer);
            

            // System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                UcWaferIDManualMainPage ucWaferIDManualMainPage = new UcWaferIDManualMainPage();
                ucWaferIDManualMainPage.DataContext = _VM;

                Window window = new Window()
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStyle = WindowStyle.None,
                    Background = Brushes.Black,
                    Height = 500,
                    Width = 800,
                    Content = ucWaferIDManualMainPage,
                    Title = "WaferIDManualInput"
                };

                _VM.SetWindow(window);
                window.ShowDialog();

                //OCR Fail Check 
            });


        }
    }
}
