using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace MachineSettingLoaderView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MachineSettingLoaderView : UserControl, IMainScreenView
    {
        public MachineSettingLoaderView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("647dc97d-99ab-4355-b7ac-1976d322e900");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
