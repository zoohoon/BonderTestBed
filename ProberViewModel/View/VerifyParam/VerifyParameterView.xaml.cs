namespace ProberViewModel.View.VerifyParam
{
    using System;
    using System.Windows.Controls;
    using ProberInterfaces;
    /// <summary>
    /// VerifyParameterView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VerifyParameterView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("23b429a7-1ce8-4897-9eeb-d01b6a106714");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public VerifyParameterView()
        {
            InitializeComponent();
        }
    }
}
