using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace OCRSubSettingView
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class OCRSubSettingView : UserControl, IMainScreenView
    {
        public OCRSubSettingView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("88573974-bdc7-4a80-b399-6fc44852122b");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
