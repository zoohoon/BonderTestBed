using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace SemicsOCRSubSettingView
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SemicsOCRSubSettingView : UserControl, IMainScreenView
    {
        public SemicsOCRSubSettingView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("267933e9-b956-4c26-93ae-f0dd0c78abe1");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
