namespace ProberViewModel.View.TesterInterface
{
    using ProberInterfaces;
    using System;
    using System.Windows.Controls;
    /// <summary>
    /// UCTaskManagement.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TesterInterfaceView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("62567D35-0DA9-4A65-884C-079A4A693916");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public TesterInterfaceView()
        {
            InitializeComponent();
        }
    }
}


