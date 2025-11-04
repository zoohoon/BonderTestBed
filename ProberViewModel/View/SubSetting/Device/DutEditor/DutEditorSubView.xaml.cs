using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace DutEditorSubView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DutEditorSubView : UserControl, IMainScreenView
    {
        public DutEditorSubView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("016b67d9-9d63-471a-97af-075abc3a841e");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
