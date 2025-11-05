using System;
using System.Windows.Controls;
using ProberInterfaces;

namespace OperatorControl
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OperatorView : UserControl, IMainScreenView
    {
        public OperatorView()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("d1bc0f1e-36b7-4508-b9c0-c09f08a5587c");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
