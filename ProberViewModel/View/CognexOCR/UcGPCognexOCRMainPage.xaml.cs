using System;
using System.Windows.Controls;

namespace GPCognexOCRMainPageView
{
    using ProberInterfaces;

    /// <summary>
    /// UcCognexOCRMainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcGPCognexOCRMainPage : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("42D9D35A-D5E6-4799-B0AE-03F9900B52C3");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public UcGPCognexOCRMainPage()
        {
            InitializeComponent();
        }
    }
}
