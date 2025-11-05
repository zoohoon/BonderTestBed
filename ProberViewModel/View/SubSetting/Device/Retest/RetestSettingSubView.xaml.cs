using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace RetestSettingSubView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RetestSettingSubView : UserControl, IMainScreenView
    {
        public RetestSettingSubView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("71d8a533-6b00-48f1-ae84-da3af07a308e");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
