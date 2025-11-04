using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace LoaderFileTransferViewModule
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderFileTransferView : UserControl, IMainScreenView
    {
        public LoaderFileTransferView()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("26e6e48b-d06a-43f9-9efc-efd1a21ea493");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
