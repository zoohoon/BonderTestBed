using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace TesterCommnuication
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TesterCommnuicationSettingView : UserControl, IMainScreenView
    {
        public TesterCommnuicationSettingView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("1ecfc450-fe45-4090-bc22-06da394fe9b1");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
