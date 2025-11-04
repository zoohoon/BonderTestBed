using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace VisionMappingSubView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VisionMappingSubView : UserControl, IMainScreenView
    {
        public VisionMappingSubView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("3cf6c16e-a992-47a7-a923-b77297c5e7b7");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
