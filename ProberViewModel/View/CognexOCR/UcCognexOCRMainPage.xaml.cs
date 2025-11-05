using System;
using System.Windows;
using System.Windows.Controls;

namespace CognexOCRMainPageView
{
    using Cognex.Controls;
    using ProberInterfaces;
    /// <summary>
    /// UcCognexOCRMainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcCognexOCRMainPage : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("8aa6dac9-5c54-43e3-8802-0a5dd69e25c8");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public UcCognexOCRMainPage()
        {
            InitializeComponent();

            if (SystemManager.SysteMode == SystemModeEnum.Multiple)
            {
                return;
            }

            //UserControl_Loaded(null, null);

            InSightDisplayApp.SetLoaderContainer(this.LoaderController()?.GetLoaderContainer());
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
            //if(this.CognexDisplay.Children.Contains(InSightDisplayApp.Get()))
            //    this.CognexDisplay.Children.Remove(InSightDisplayApp.Get());
        }
    }
}
