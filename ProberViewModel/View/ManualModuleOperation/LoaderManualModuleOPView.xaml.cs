using System;
using System.Windows.Controls;

namespace LoaderManualModuleOPViewModule
{
    using ProberInterfaces;
    /// <summary>
    /// LoaderManualModuleOPView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderManualModuleOPView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("3E34A0EA-05F2-029E-6D2D-DFCF43922121");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public LoaderManualModuleOPView()
        {
            InitializeComponent();
        }
    }
}
