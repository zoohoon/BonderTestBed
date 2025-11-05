using System;
using System.Windows.Controls;

namespace UcMotorsView
{
    using ProberInterfaces;
    public partial class MotorsView : UserControl, IMainScreenView
    {
        public MotorsView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("7bd6a1b7-8d90-4dee-8de7-633bb966de6a");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
