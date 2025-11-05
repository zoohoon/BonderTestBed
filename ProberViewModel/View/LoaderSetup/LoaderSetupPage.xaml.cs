using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace LoaderSetupPageView
{
    /// <summary>
    /// SetupPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderSetupPage : UserControl, IMainScreenView
    {
        public LoaderSetupPage()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("31211D6F-B8A1-16C6-04EA-1656F17C4C54");
        public Guid ScreenGUID { get { return _ViewGUID; } }

    }
}
