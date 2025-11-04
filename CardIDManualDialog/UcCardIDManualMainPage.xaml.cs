using ProberInterfaces;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CardIDManualDialog
{
    /// <summary>
    /// UcCardIDManualMainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcCardIDManualMainPage  : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("89d45166-a25c-4200-8204-2cfe5445dcc6");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public UcCardIDManualMainPage()
        {
            InitializeComponent();

            if (SystemManager.SysteMode == SystemModeEnum.Multiple)
            {
                return;
            }
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
    }
}
