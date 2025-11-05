using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ProberViewModel.View.LogView
{
    /// <summary>
    /// LogViewerView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogViewerView : UserControl, IMainScreenView
    {
        public LogViewerView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("53201f06-8810-4e46-b2b0-e748dc9d97f1");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
