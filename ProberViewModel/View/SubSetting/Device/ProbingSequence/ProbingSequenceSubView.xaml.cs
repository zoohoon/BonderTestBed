using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ProbingSequenceSubView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProbingSequenceSubView : UserControl, IMainScreenView
    {
        public ProbingSequenceSubView()
        {
            InitializeComponent();
        }
        private readonly Guid _ViewGUID = new Guid("f83a2fc4-3e01-4b13-ae84-b59acd8e1a26");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
