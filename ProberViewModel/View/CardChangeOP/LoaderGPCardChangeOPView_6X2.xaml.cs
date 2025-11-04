using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ProberViewModel
{
    /// <summary>
    /// LoaderGPCardChageOPView_6X2.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderGPCardChangeOPView_6X2 : UserControl, IMainScreenView
    {
        public LoaderGPCardChangeOPView_6X2()
        {
            InitializeComponent();
        }
        readonly Guid _ViewGUID = new Guid("6c647706-8060-447a-992e-6c8893b0f025");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
