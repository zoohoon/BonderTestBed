using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace UcGemSysSettingView
{
    public partial class GemSysSettingView : UserControl, IMainScreenView
    {
        public GemSysSettingView()
        {
            InitializeComponent();
        }

        public Guid ScreenGUID { get; set; } = new Guid("3579E2AA-BB0D-48DB-8229-BCF66A6044A0");
    }
}
