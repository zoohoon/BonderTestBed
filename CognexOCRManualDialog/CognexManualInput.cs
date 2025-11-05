using System;

namespace CognexOCRManualDialog
{
    using LampModule;
    using ProberInterfaces;
    using System.Windows;
    using System.Windows.Media;

    public static class CognexManualInput
    {
        private static VmCognexOCRManualMainPage _VM;
        private static VmGPCognexOCRManualMainPage _GP_VM;

        public static void AsyncShow(Autofac.IContainer container)
        {
            STAThread(container);
        }
        //public static void AsyncShow(Autofac.IContainer LoaderContainer,int idx)
        //{
        //    STAAyncThread(LoaderContainer, idx);
        //}

        public static void Show(Autofac.IContainer LoaderContainer, int idx)
        {

            STAThread(LoaderContainer, idx);
        }

        private static void STAThread(Autofac.IContainer container)
        {
            _VM = new VmCognexOCRManualMainPage();
            if (_VM.IsWaferExistOnOCR() == false)
                return;

            _VM.InitModule();
            _VM.InitViewModel();
            _VM.PageSwitched();

            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UcSingleCognexOCRManualMainPage ucCognexOCRManualMainPage = new UcSingleCognexOCRManualMainPage();
                ucCognexOCRManualMainPage.DataContext = _VM;

                Window window = new Window()
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStyle = WindowStyle.None,
                    Background = Brushes.Black,
                    Height = 850,
                    Width = 800,
                    Content = ucCognexOCRManualMainPage,
                    Title = "CognexManualInput"
                };

                using (LampBarrier lampBarrier = new LampBarrier(
                    LampStatusEnum.Off,
                    LampStatusEnum.BlinkOn,
                    LampStatusEnum.Off,
                    LampStatusEnum.On,
                    AlarmPriority.Warning, sender: "Cognex Manual"))
                {
                }
                _VM.SetWindow(window);
                window.ShowDialog();
            }));
        }


        private static void STAAyncThread(Autofac.IContainer LoaderContainer, int index)
        {
            _GP_VM = new VmGPCognexOCRManualMainPage();
            _GP_VM._Container = LoaderContainer;
            _GP_VM.InitModule(index);
            _GP_VM.InitViewModel();
            _GP_VM.PageSwitched();

            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
           {
               UcCognexOCRManualMainPage ucCognexOCRManualMainPage = new UcCognexOCRManualMainPage();
               ucCognexOCRManualMainPage.DataContext = _GP_VM;

               Window window = new Window()
               {
                   WindowStartupLocation = WindowStartupLocation.CenterScreen,
                   ResizeMode = ResizeMode.NoResize,
                   WindowStyle = WindowStyle.None,
                   Background = Brushes.Black,
                   Height = 940,
                   Width = 800,
                   Content = ucCognexOCRManualMainPage
               };

               _GP_VM.SetWindow(window);
               window.ShowDialog();
           }));


        }

        private static void STAThread(Autofac.IContainer LoaderContainer, int index)
        {
            _GP_VM = new VmGPCognexOCRManualMainPage();
            _GP_VM._Container = LoaderContainer;
            _GP_VM.InitModule(index);
            _GP_VM.InitViewModel();
            _GP_VM.PageSwitched();

            // System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                UcCognexOCRManualMainPage ucCognexOCRManualMainPage = new UcCognexOCRManualMainPage();
                ucCognexOCRManualMainPage.DataContext = _GP_VM;

                Window window = new Window()
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStyle = WindowStyle.None,
                    Background = Brushes.Black,
                    Height = 940,
                    Width = 800,
                    Content = ucCognexOCRManualMainPage,
                    Title = "CognexManualInput"
                };

                _GP_VM.SetWindow(window);
                window.ShowDialog();

                //OCR Fail Check 
            });


        }
    }
}
