using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace SoakingSettingView
{
    /// <summary>
    /// UcSoakingOD.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcSoakingOD : UserControl,IMainScreenView
    {
        public UcSoakingOD()
        {
            InitializeComponent();
        }
        readonly Guid _ViewGUID = new Guid("CA928067-AB08-4E57-90EF-A6D549625942");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
