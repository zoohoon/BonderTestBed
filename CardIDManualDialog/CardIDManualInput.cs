using ProberInterfaces;
using System;
using System.Windows;
using System.Windows.Media;

namespace CardIDManualDialog
{
    public static class CardIDManualInput
    {
        private static VmCardIDMainPage _VM;

        public static void Show(Autofac.IContainer LoaderContainer, TransferObject transfer = null)
        {

            STAThread(LoaderContainer, transfer);
        }
     
        private static void STAThread(Autofac.IContainer LoaderContainer,TransferObject transfer=null)
        {
            _VM = new VmCardIDMainPage();

            _VM.InitModule(transfer);
            _VM._Container = LoaderContainer;

            // System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                UcCardIDManualMainPage ucCardIDManualMainPage = new UcCardIDManualMainPage();
                ucCardIDManualMainPage.DataContext = _VM;

                Window window = new Window()
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStyle = WindowStyle.None,
                    Background = Brushes.Black,
                    Height = 500,
                    Width = 800,
                    Content = ucCardIDManualMainPage,
                    Title = "CardIDManualInput"
                };

                _VM.SetWindow(window);
                window.ShowDialog();

                //OCR Fail Check 
            });


        }
    }
}
