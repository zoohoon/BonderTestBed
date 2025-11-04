using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CognexOCRManualDialog
{
    using Autofac;
    using Cognex.Controls;
    using ProberInterfaces;
    /// <summary>
    /// UcCognexOCRManualMainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcCognexOCRManualMainPage : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("0e592af2-bd77-4de9-9ea8-958cf98553cc");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public UcCognexOCRManualMainPage()
        {
            InitializeComponent();

            if (SystemManager.SysteMode == SystemModeEnum.Multiple)
            {
                return;
            }

            //UserControl_Loaded(null, null);


            InSightDisplayApp.SetLoaderContainer(this.LoaderController().GetLoaderContainer());
            InSightDisplayApp.Get();
        }


        //==> Live 영상 출력해야 할 때 필요
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //if (InSightDisplayApp.Get().Parent == null)
            //    this.CognexDisplay.Children.Add(InSightDisplayApp.Get());
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //if (this.CognexDisplay.Children.Contains(InSightDisplayApp.Get()))
            //    this.CognexDisplay.Children.Remove(InSightDisplayApp.Get());
        }

        private void exitBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            exitBtn.Opacity = 0.5;
        }

        private void exitBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            exitBtn.Opacity = 1;
        }

        private void exitBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            exitBtn.Opacity = 1;
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var gpLoader = this.GetLoaderContainer().Resolve<IGPLoader>();
            if(gpLoader!=null)
            {
                gpLoader.LoaderBuzzer(false);
            }
        }
    }
}
