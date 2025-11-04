using System;
using System.Windows.Controls;

namespace LoaderTestMainPageView
{
    using ProberInterfaces;
    /// <summary>
    /// UcLoaderTestMainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcLoaderTestMainPage : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("f13232cf-b840-4ef1-92e3-c198c5da42ec");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public UcLoaderTestMainPage()
        {
            InitializeComponent();
        }
    }
}
