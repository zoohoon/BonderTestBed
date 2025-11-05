using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace FileSystemView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FileSystemView : UserControl, IMainScreenView
    {
        public FileSystemView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("8cf01c41-628c-4ea9-bba2-38a6f934e5fc");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
