using System;
using System.Windows;
using System.Windows.Controls;

namespace GPCardChangeMainPageView
{
    using ProberInterfaces;
    /// <summary>
    /// UcGPCardChangeMainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcGPCardChangeMainPage : UserControl, IMainScreenView, IFactoryModule
    {
        public UcGPCardChangeMainPage()
        {
            InitializeComponent();
        }
        readonly Guid _ViewGUID = new Guid("b7104207-1f96-4669-b027-03061794d5a5");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            //==> Display port 화면을 출력 시키기 위해.
            this.VisionManager()?.SetDisplayChannelStageCameras(displayport);
            
        }
    }
}
